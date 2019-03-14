using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Web.Configuration;
using System.Globalization;
using System.Data;
using System.Transactions;
using System.Data.SqlTypes;
using MySql.Data.Common;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using MySql.Data.Types;


namespace Saalut.Services
{
    /// <summary>
    /// Summary description for Cennic
    /// </summary>
    public class Cennic : IHttpHandler
    {
        int mapSize = 1;

        string connStr = global::System.Configuration.ConfigurationManager.ConnectionStrings["MySqlServerConnectionString"].ConnectionString;



        public void ProcessRequest(HttpContext context)
        {
            string Division = WebConfigurationManager.AppSettings["Division"];


            string journalLineID = (context.Request.QueryString["LID"]).ToUpper();
            string printTmplID = (context.Request.QueryString["TID"]).ToUpper();

            string proizvoditel = (context.Request.QueryString["PID"]).ToUpper();

            SaalutDataClasses1DataContext dataContext = new SaalutDataClasses1DataContext();

            int jlID = 0;
            Int32.TryParse(journalLineID, out jlID);

            var jl = (from j in dataContext.PriceChangeLine
                      where j.ID == jlID
                      select j).FirstOrDefault();
            if (jl == null)
                return;

            var good = (from g in dataContext.Goods
                        where g.ID == jl.GoodID
                        select g).FirstOrDefault();
            if (good == null)
                return;

            // settings
            var settings = (from s in dataContext.Settings
                            select s).FirstOrDefault();

            var store = (from s in dataContext.StoreInfos
                         where s.Active == true
                         select s).FirstOrDefault();
            if (store == null)
                return;

            //+ 06052014
            var storeExt = (from s in dataContext.StoreInfoExt
                            select s).FirstOrDefault();
            //- 06052014

            int ptID = 0;
            Int32.TryParse(printTmplID, out ptID);

            //+07052014
            if (ptID == 0)
                if (good.PrintTemplateID != null)
                    ptID = good.PrintTemplateID.Value;
            //-07052014

            var pt = (from p in dataContext.PrintTemplates
                      where p.ID == ptID
                      select p).FirstOrDefault();

            if (pt == null)
            {
                // ищем шаблон на верхнем уровне.
                if (good.Group.PrintTemplateID == null)
                {
                    int upGroupID = 0;
                    if (good.Group.GroupRangeID != 0 && good.Group.GroupRangeID != null)
                        upGroupID = good.Group.GroupRangeID.Value;
                    while (upGroupID != 0)
                    {
                        var grp = (from g in dataContext.Groups
                                   where g.ID == upGroupID
                                   select g).FirstOrDefault();
                        if (grp == null)
                            continue;

                        if (grp.PrintTemplateID == null)
                        {
                            if (grp.GroupRangeID != 0 && grp.GroupRangeID != null)
                            {
                                upGroupID = grp.GroupRangeID.Value;
                                continue;
                            }
                            else
                                upGroupID = 0;
                        }
                        else
                        {
                            ptID = grp.PrintTemplateID.Value;
                            break;
                        }

                        if (good.Group.GroupRangeID != 0 && good.Group.GroupRangeID != null)
                            upGroupID = good.Group.GroupRangeID.Value;

                        if (grp.GroupRangeID == null)
                            break;
                    }
                }
                else
                    ptID = good.Group.PrintTemplateID.Value;
            }

            // проверяем нашли ли шаблон
            if (ptID == 0)
                return; // возврат если не нашли

            var template = (from t in dataContext.PrintTemplates
                            where t.ID == ptID
                            select t).FirstOrDefault();

            if (template == null)
                return;

            //+ 06052014
            var templateExt = (from t in dataContext.PrintTemplatesExt
                               where t.PrintTemplateID == ptID
                               select t).FirstOrDefault();
            //- 06052014

            Unit a = new Unit(template.ShirinaCennicaMM.Value, UnitTypes.Mm);
            int wImage = Int32.Parse(Math.Round(a.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки
            Unit b = new Unit(template.VisotaCennicaMM.Value, UnitTypes.Mm);
            int hImage = Int32.Parse(Math.Round(b.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки

            int otstupKray = 1; //отсутуп от края справа и снизу (справа может быть в 2 раза больше)

            wImage = wImage * mapSize; // увеличим размерчик для улучшения качества
            hImage = hImage * mapSize;

            int wGraph = wImage - otstupKray;
            int hGraph = hImage - otstupKray;


            Bitmap image = new Bitmap(wImage, hImage);

            // Get the physical path of the current application.
            string appPath = HttpRuntime.AppDomainAppPath;
            // Get the complete physical path of the file to read.
            string file = appPath + @"\" + template.FileName;
            Image backImage = Image.FromFile(file);


            Graphics graf = Graphics.FromImage(image);
            graf.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            //graf.SmoothingMode = SmoothingMode.None;
            //graf.InterpolationMode = InterpolationMode.Low;
            //graf.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
            //graf.PixelOffsetMode = PixelOffsetMode.None;

            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush blackBrush = new SolidBrush(Color.Black);


            graf.FillRectangle(blackBrush, 0, 0, wImage, hImage);
            if (template.TemplateName.Contains("акционный"))
                graf.FillRectangle(whiteBrush, 0, 0, wImage, hImage);
            else
                graf.FillRectangle(whiteBrush, 1, 1, wImage - 2, hImage - 2);


            // Информация ценника

            if (template.GoodNameFontWeight.Value != 0)
            {
                string descr = good.Descr;
                StringBuilder str = new StringBuilder();

                descr = descr.Replace("\r\n", "\n");
                descr = descr.Replace("\r", "\n");
                descr = descr.Replace("\t", " ");


                int length = descr.Length;
                int from = 0;

                int strok = 3; //не более 3 строк в наименование
                while (length >= template.GoodNameSimvolovVStr)
                {
                    str.Append(descr.Substring(from, template.GoodNameSimvolovVStr.Value) + "\n");
                    from += template.GoodNameSimvolovVStr.Value;
                    length -= template.GoodNameSimvolovVStr.Value;

                    if (strok == 1)
                        break;
                    strok--;
                }
                if (length < template.GoodNameSimvolovVStr)
                    str.Append(descr.Substring(from, length));

                string[] textParagraphs = str.ToString().Trim().Split('\n');


                Font fontGoodName = new Font(FontFamily.GenericSansSerif, template.GoodNameFontWeight.Value * mapSize, FontStyle.Bold);
                //Font fontGoodName = new Font("Arial", 96 / 8, FontStyle.Bold);
                int i = 0;
                foreach (string strText in textParagraphs.ToArray())
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        if (template.GoodNameCentrovano == true)
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                        }
                        else
                        {
                            sf.Alignment = StringAlignment.Near;
                            sf.LineAlignment = StringAlignment.Near;
                        }

                        sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                        sf.FormatFlags = StringFormatFlags.NoWrap;

                        int otstPx = 0;
                        if (i != 0)
                        {
                            Unit o = new Unit(template.GoodNameOtstupMM.Value, UnitTypes.Mm);
                            otstPx = Int32.Parse(Math.Round(o.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки
                        }
                        graf.DrawString(strText.Trim(), fontGoodName, blackBrush, template.GoodNameX.Value * mapSize, (template.GoodNameY.Value + (i * otstPx)) * mapSize, sf);

                    }
                    i++;
                }


            }


            // + цена по новому
            DataTable prices;

            MySqlConnection cnx = null;
            try
            {
                cnx = new MySqlConnection(connStr);
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                // Prices
                string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where item = '" + good.Articul + "' and pricelist_id = '" + store.PriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                MySqlCommand cmd = new MySqlCommand(cmdText, cnx);
                cmd.CommandTimeout = 30000;

                // Create a fill a Dataset
                DataSet ds5 = new DataSet();
                adapter.SelectCommand = cmd;
                adapter.Fill(ds5);

                prices = ds5.Tables[0];

            }
            catch (MySqlException ex)
            {
                return;
            }
            finally
            {
                if (cnx != null)
                {
                    cnx.Close();
                }
            }

            // - цена по новому


            string priceVal = "нет цены";
            //var price = (from p in dataContext.Prices
            //             where p.GoodID == good.ID
            //             && p.Active == true
            //             select p).FirstOrDefault();
            //if (price != null)
            //{
            //    double priceFromDB = price.Price1.Value;

            //    if (template.EdinicZa100Gr != null)
            //    {
            //        if (template.EdinicZa100Gr.Value)
            //        {
            //            priceFromDB = priceFromDB / 10;
            //        }
            //    }
            //    priceVal = priceFromDB.ToString();
            //}

            // + новая цена
            decimal price = 0;
            if (prices != null)
            {
                foreach (DataRow row in prices.Rows)
                {
                    price = (decimal)row[1];
                }
            }
            if (price != 0)
            {
                if (template.EdinicZa100Gr != null)
                {
                    if (template.EdinicZa100Gr.Value)
                    {
                        price = price / 10;
                    }
                }

                priceVal = price.ToString();
            }
            // - новая цена

            string priceRub = "0";
            string priceKop = "00";

            int priceValZap = priceVal.IndexOf(",");
            if (priceValZap != -1)
            {
                priceRub = priceVal.Substring(0, priceValZap);
                priceKop = priceVal.Substring(priceValZap + 1, priceVal.Length - priceValZap - 1);
            }
            else
            {
                priceValZap = priceVal.IndexOf(".");
                if (priceValZap != -1)
                {
                    priceRub = priceVal.Substring(0, priceValZap);
                    priceKop = priceVal.Substring(priceValZap + 1, priceVal.Length - priceValZap - 1);
                }
                else
                    priceRub = priceVal;
            }

            if (priceKop.Length == 1)
                priceKop += "0";

            if (priceKop.Length >= 3)
                priceKop = priceKop.Substring(0, 2);

            if (price != 0)
            {
                int iPriceRub = Int32.Parse(priceRub);
                priceRub = iPriceRub.ToString("N0", CultureInfo.CreateSpecificCulture("ru-RU"));

                if (Division == "RF")
                {
                    if (template.PriceRubFromRightToLeft == false)
                    {
                        if (priceRub.Length == 1)
                            priceRub = "   " + priceRub;
                        if (priceRub.Length == 2)
                            priceRub = "  " + priceRub;
                        if (priceRub.Length == 3)
                            priceRub = " " + priceRub;
                    }
                    else
                    {
                        if (priceRub.Length == 1)
                            priceRub = "     " + priceRub;
                        if (priceRub.Length == 2)
                            priceRub = "    " + priceRub;
                        if (priceRub.Length == 3)
                            priceRub = "  " + priceRub;
                        if (priceRub.Length == 4)
                            priceRub = " " + priceRub;
                    }

                }
                else
                    if (Division == "RB")
                    {
                        if (priceRub.Length == 2)
                            priceRub = "      " + priceRub;
                        if (priceRub.Length == 3)
                            priceRub = "     " + priceRub;
                        if (priceRub.Length == 4)
                            priceRub = "     " + priceRub;
                        if (priceRub.Length == 5)
                            priceRub = "    " + priceRub;
                        if (priceRub.Length == 6)
                            priceRub = "   " + priceRub;
                        if (priceRub.Length == 7)
                            priceRub = "  " + priceRub;
                    }
            }

            if (template.PriceRubFontWeight.Value != 0)
            {
                Font fontPriceRub = new Font("Arial black", template.PriceRubFontWeight.Value * mapSize, FontStyle.Bold);
                //Font fontPriceRub = new Font("Arial black", 64, FontStyle.Bold);
                graf.DrawString(priceRub, fontPriceRub, blackBrush, template.PriceRubX.Value * mapSize, template.PriceRubY.Value * mapSize);
            }

            if (template.SlovoRubFontWeigh.Value != 0)
            {
                Font font = new Font(FontFamily.GenericSansSerif, template.SlovoRubFontWeigh.Value * mapSize, FontStyle.Regular);
                if (template.SlovoRubUnderline.Value == true)
                    font = new Font(FontFamily.GenericSansSerif, template.SlovoRubFontWeigh.Value * mapSize, FontStyle.Underline);
                graf.DrawString("РУБ", font, blackBrush, template.SlovoRubX.Value * mapSize, template.SlovoRubY.Value * mapSize);
            }

            if (template.PriceKopFontWeight.Value != 0 && Division != "RB")
            {
                Font fontPriceKop = new Font("Arial black", template.PriceKopFontWeight.Value * mapSize, FontStyle.Bold);
                graf.DrawString(priceKop, fontPriceKop, blackBrush, template.PriceKopX.Value * mapSize, template.PriceKopY.Value * mapSize);
            }

            if (template.SlovoKopFontWeight.Value != 0 && Division != "RB")
            {
                Font font = new Font(FontFamily.GenericSansSerif, template.SlovoKopFontWeight.Value * mapSize, FontStyle.Regular);
                if (template.SlovoKopUnderline.Value == true)
                    font = new Font(FontFamily.GenericSansSerif, template.SlovoKopFontWeight.Value * mapSize, FontStyle.Underline);
                graf.DrawString("КОП", font, blackBrush, template.SlovoKopX.Value * mapSize, template.SlovoKopY.Value * mapSize);
            }

            if (!template.EdinicZa100Gr.Value && template.EdinicFontWeight.Value != 0)
            {
                Font fontEdinic = new Font(FontFamily.GenericSansSerif, template.EdinicFontWeight.Value * mapSize, FontStyle.Regular);
                graf.DrawString("ед.изм.: " + good.Edinic, fontEdinic, blackBrush, template.EdinicX.Value * mapSize, template.EdinicY.Value * mapSize);
            }

            if (template.EdinicZa100Gr.Value && template.EdinicFontWeight.Value != 0)
            {
                Font fontEdinic = new Font(FontFamily.GenericSansSerif, template.EdinicFontWeight.Value * mapSize, FontStyle.Regular);
                graf.DrawString("Цена за 100 гр.", fontEdinic, blackBrush, template.EdinicX.Value * mapSize, template.EdinicY.Value * mapSize);
            }

            if (template.SostavFontWeight.Value != 0)
            {

                string Contents = good.Contents.Trim();

                if (Contents != "")
                {
                    if (Contents.Substring(0, 6).ToUpper() != "СОСТАВ")
                        Contents = "Состав: " + Contents;
                }

                StringBuilder str = new StringBuilder();

                Contents = Contents.Replace("\r\n", "\n");
                Contents = Contents.Replace("\r", "\n");
                Contents = Contents.Replace("\t", " ");


                int length = Contents.Length;
                int from = 0;


                while (length >= template.SostavSimvolovVStr)
                {
                    str.Append(Contents.Substring(from, template.SostavSimvolovVStr.Value) + "\n");
                    from += template.SostavSimvolovVStr.Value;
                    length -= template.SostavSimvolovVStr.Value;
                }
                if (length < template.SostavSimvolovVStr)
                    str.Append(Contents.Substring(from, length));

                string[] textParagraphs = str.ToString().Trim().Split('\n');


                Font fontSostav = new Font(FontFamily.GenericSansSerif, template.SostavFontWeight.Value * mapSize, FontStyle.Regular);

                int i = 0;
                foreach (string strText in textParagraphs.ToArray())
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Near;
                        sf.LineAlignment = StringAlignment.Near;
                        sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                        sf.FormatFlags = StringFormatFlags.NoWrap;

                        int otstPx = 0;
                        if (i != 0)
                        {
                            Unit o = new Unit(template.SostavOtstupMM.Value, UnitTypes.Mm);
                            otstPx = Int32.Parse(Math.Round(o.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки
                        }
                        graf.DrawString(strText.Trim(), fontSostav, blackBrush, template.SostavX.Value * mapSize, (template.SostavY.Value + (i * otstPx)) * mapSize);
                    }
                    i++;
                }

            }



            if (template.ProizvoditelFontWeight.Value != 0)
            {
                string contsts = good.Producer;
                if (contsts.Length > 0)
                {
                    contsts = contsts.Substring(0, 1).ToUpper() + contsts.Substring(1).ToLower();
                }
                contsts += " " + proizvoditel;
                Font fontProizvoditel = new Font(FontFamily.GenericSansSerif, template.ProizvoditelFontWeight.Value * mapSize, FontStyle.Regular);
                graf.DrawString((contsts).Trim(), fontProizvoditel, blackBrush, template.ProizvoditeX.Value * mapSize, template.ProizvoditeY.Value * mapSize);
            }

            if (template.MagazinFontWeight.Value != 0)
            {
                if (store != null)
                {
                    Font fontMagazin = new Font(FontFamily.GenericSansSerif, template.MagazinFontWeight.Value * mapSize, FontStyle.Regular);
                    graf.DrawString(store.StoreName.Trim(), fontMagazin, blackBrush, template.MagazinX.Value * mapSize, template.MagazinY.Value * mapSize);
                }
            }

            if (template.JurlicoFontWeight.Value != 0)
            {
                if (store != null)
                {
                    if (store.Company != null)
                    {
                        Font font = new Font(FontFamily.GenericSansSerif, template.JurlicoFontWeight.Value * mapSize, FontStyle.Regular);
                        graf.DrawString(store.Company.Trim(), font, blackBrush, template.JurlicoX.Value * mapSize, template.JurlicoY.Value * mapSize);
                    }
                }
            }

            if (template.FactAddressFontWeight.Value != 0)
            {
                if (store != null)
                {
                    if (store.AddressFact != null)
                    {
                        Font fontAddressFact = new Font(FontFamily.GenericSansSerif, template.FactAddressFontWeight.Value * mapSize, FontStyle.Regular);
                        graf.DrawString(store.AddressFact.Trim(), fontAddressFact, blackBrush, template.FactAddressX.Value * mapSize, template.FactAddressY.Value * mapSize);
                    }
                }
            }

            if (template.JurAddressFontWeight.Value != 0)
            {
                if (store != null)
                {
                    if (store.AddressJur != null)
                    {
                        Font fontAddressJur = new Font(FontFamily.GenericSansSerif, template.JurAddressFontWeight.Value * mapSize, FontStyle.Regular);
                        graf.DrawString(store.AddressJur.Trim(), fontAddressJur, blackBrush, template.JurAddressX.Value * mapSize, template.JurAddressY.Value * mapSize);
                    }
                }
            }

            if (template.DataFontWeight.Value != 0)
            {
                Font fontData = new Font(FontFamily.GenericSansSerif, template.DataFontWeight.Value * mapSize, FontStyle.Regular);
                graf.DrawString("Дата:" + DateTime.Today.ToString("dd.MM"), fontData, blackBrush, template.DataX.Value * mapSize, template.DataY.Value * mapSize);
            }


            //Image img = image.GetThumbnailImage(wImage / mapSize, hImage / mapSize, null, (new System.IntPtr(0)));


            if (template.BarcodeShirinaMM.Value != -1 && template.BarcodeVisotaMM.Value != -1)
            {
                Graphics graf2 = Graphics.FromImage(image);

                string barcode = good.Barcode;
                if (barcode == "" || barcode == "0" || barcode == null) // bug fix
                {
                    var barkodes_all = (from brk in dataContext.Barcodes
                                        where brk.GoodID == good.ID
                                        && brk.Active == true
                                        select brk).FirstOrDefault();
                    if (barkodes_all != null)
                        barcode = barkodes_all.Barcode1;
                }

                //bug fix
                if (barcode == "" || barcode == "0" || barcode == null)
                {
                    UKMDataBaseConnects utl = new UKMDataBaseConnects();
                    utl.UpdateGood(good, good.Articul);
                    //2
                    utl.UpdateGood(good, good.Articul);
                }

                if (barcode.Length >= 13)
                    barcode = barcode.Substring(0, 12);

                if (barcode.Length < 12)
                {
                    int ii = 12 - barcode.Length;
                    for (int i = 1; i <= ii; i++)
                        barcode += "0";
                }

                BarcodeEAN13 encoderEAN13 = new BarcodeEAN13();
                Image barcImg = encoderEAN13.Encode(barcode);

                Rectangle nr = new Rectangle(0, barcImg.Height / 2, barcImg.Width, barcImg.Height / 3);

                if (settings != null)
                {
                    if (template.BarcodeShirinaMM.Value != 0 && template.BarcodeVisotaMM.Value != 0)
                    {
                        //if (settings.ResizeBarcodes.Value)
                        //{
                        Unit c = new Unit(template.BarcodeShirinaMM.Value, UnitTypes.Mm);
                        int wBC = Int32.Parse(Math.Round(c.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки
                        Unit d = new Unit(template.BarcodeVisotaMM.Value, UnitTypes.Mm);
                        int hBC = Int32.Parse(Math.Round(d.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки

                        graf2.DrawImage(this.ResizeImg(barcImg, wBC, hBC), new Point(template.BarcodeX.Value, template.BarcodeY.Value));
                    }
                    else
                    {
                        if (templateExt != null)
                        {
                            if (templateExt.MalenkiyBarcode.Value == true)
                                graf2.DrawImage(this.cropImage(barcImg, nr), new Point(template.BarcodeX.Value, template.BarcodeY.Value));
                            else
                                graf2.DrawImage(barcImg, new Point(template.BarcodeX.Value, template.BarcodeY.Value));
                        }
                        else
                            graf2.DrawImage(this.cropImage(barcImg, nr), new Point(template.BarcodeX.Value, template.BarcodeY.Value));
                    }
                }
                else
                    graf2.DrawImage(barcImg, new Point(template.BarcodeX.Value, template.BarcodeY.Value));
            }


            //+ 06052014 PrintTemplateExt
            decimal priceOld = 0;
            if (storeExt != null)
            {
                DataTable pricesOld;

                MySqlConnection cnxOld = null;
                try
                {
                    cnxOld = new MySqlConnection(connStr);
                    MySqlDataAdapter adapterOld = new MySqlDataAdapter();

                    // Prices
                    string cmdText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ; select item, price, version, deleted from ukmserver.trm_in_pricelist_items where item = '" + good.Articul + "' and pricelist_id = '" + storeExt.OsnPriceList_ID_UKM.ToString() + "'  and deleted = 0; SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ ; ";
                    MySqlCommand cmd = new MySqlCommand(cmdText, cnxOld);
                    cmd.CommandTimeout = 30000;

                    // Create a fill a Dataset
                    DataSet ds5 = new DataSet();
                    adapterOld.SelectCommand = cmd;
                    adapterOld.Fill(ds5);

                    pricesOld = ds5.Tables[0];

                }
                catch (MySqlException ex)
                {
                    return;
                }
                finally
                {
                    if (cnxOld != null)
                    {
                        cnxOld.Close();
                    }
                }

                string priceValOld = "";
                //var price = (from p in dataContext.Prices
                //             where p.GoodID == good.ID
                //             && p.Active == true
                //             select p).FirstOrDefault();
                //if (price != null)
                //{
                //    double priceFromDB = price.Price1.Value;

                //    if (template.EdinicZa100Gr != null)
                //    {
                //        if (template.EdinicZa100Gr.Value)
                //        {
                //            priceFromDB = priceFromDB / 10;
                //        }
                //    }
                //    priceVal = priceFromDB.ToString();
                //}


                if (pricesOld != null)
                {
                    foreach (DataRow row in pricesOld.Rows)
                    {
                        priceOld = (decimal)row[1];
                    }
                }
                if (priceOld != 0)
                {
                    if (template.EdinicZa100Gr != null)
                    {
                        if (template.EdinicZa100Gr.Value)
                        {
                            priceOld = priceOld / 10;
                        }
                    }

                    priceValOld = priceOld.ToString();
                }


                string priceRubOld = "0";
                string priceKopOld = "00";

                int priceValZapOld = priceValOld.IndexOf(",");
                if (priceValZapOld != -1)
                {
                    priceRubOld = priceValOld.Substring(0, priceValZapOld);
                    priceKopOld = priceValOld.Substring(priceValZapOld + 1, priceValOld.Length - priceValZapOld - 1);
                }
                else
                {
                    priceValZapOld = priceValOld.IndexOf(".");
                    if (priceValZapOld != -1)
                    {
                        priceRubOld = priceValOld.Substring(0, priceValZapOld);
                        priceKopOld = priceValOld.Substring(priceValZapOld + 1, priceValOld.Length - priceValZapOld - 1);
                    }
                    else
                        priceRubOld = priceValOld;
                }

                if (priceKopOld.Length == 1)
                    priceKopOld += "0";

                if (priceKopOld.Length >= 3)
                    priceKopOld = priceKopOld.Substring(0, 2);

                if (priceOld != 0)
                {
                    int iPriceRubOld = Int32.Parse(priceRubOld);
                    priceRubOld = iPriceRubOld.ToString("N0", CultureInfo.CreateSpecificCulture("ru-RU"));

                    if (Division == "RF")
                    {
                        if (priceRubOld.Length == 1)
                            priceRubOld = "   " + priceRubOld;
                        if (priceRubOld.Length == 2)
                            priceRubOld = "  " + priceRubOld;
                        if (priceRubOld.Length == 3)
                            priceRubOld = " " + priceRubOld;
                    }
                    else
                        if (Division == "RB")
                        {
                            if (priceRubOld.Length == 2)
                                priceRubOld = "      " + priceRubOld;
                            if (priceRubOld.Length == 3)
                                priceRubOld = "     " + priceRubOld;
                            if (priceRubOld.Length == 4)
                                priceRubOld = "     " + priceRubOld;
                            if (priceRubOld.Length == 5)
                                priceRubOld = "    " + priceRubOld;
                            if (priceRubOld.Length == 6)
                                priceRubOld = "   " + priceRubOld;
                            if (priceRubOld.Length == 7)
                                priceRubOld = "  " + priceRubOld;
                        }
                }

                if (template.OldPriceRubFontWeight.Value != 0)
                {
                    Font fontPriceRub = new Font(FontFamily.GenericSansSerif, template.OldPriceRubFontWeight.Value * mapSize, FontStyle.Regular);
                    graf.DrawString(priceRubOld, fontPriceRub, blackBrush, template.OldPriceRubX.Value * mapSize, template.OldPriceRubY.Value * mapSize);
                }

                if (template.OldSlovoRubFontWeigh.Value != 0)
                {
                    Font font = new Font(FontFamily.GenericSansSerif, template.OldSlovoRubFontWeigh.Value * mapSize, FontStyle.Regular);
                    graf.DrawString("РУБ", font, blackBrush, template.OldSlovoRubX.Value * mapSize, template.OldSlovoRubY.Value * mapSize);
                }

                if (template.OldPriceKopFontWeight.Value != 0 && Division != "RB")
                {
                    Font fontPriceKop = new Font(FontFamily.GenericSansSerif, template.OldPriceKopFontWeight.Value * mapSize, FontStyle.Regular);
                    graf.DrawString(priceKopOld, fontPriceKop, blackBrush, template.OldPriceKopX.Value * mapSize, template.OldPriceKopY.Value * mapSize);
                }

                if (template.OldSlovoKopFontWeight.Value != 0 && Division != "RB")
                {
                    Font font = new Font(FontFamily.GenericSansSerif, template.OldSlovoKopFontWeight.Value * mapSize, FontStyle.Regular);

                    graf.DrawString("КОП", font, blackBrush, template.OldSlovoKopX.Value * mapSize, template.OldSlovoKopY.Value * mapSize);
                }

            }

            if (templateExt != null)
            {
                if (templateExt.SlovoCenaFontWeight.Value != 0)
                {
                    Font fontData = new Font(FontFamily.GenericSansSerif, templateExt.SlovoCenaFontWeight.Value * mapSize, FontStyle.Regular);
                    graf.DrawString(templateExt.SlovoCena, fontData, blackBrush, templateExt.SlovoCenaX.Value * mapSize, templateExt.SlovoCenaY.Value * mapSize);
                }
                if (templateExt.DopTextFontWeight.Value != 0)
                {
                    double priceOldD = Double.Parse(priceOld.ToString());
                    double priceD = Double.Parse(price.ToString());

                    double percentDec = (priceOldD - priceD) / priceOldD * 100;
                    percentDec = Math.Round(percentDec);

                    Font fontData = new Font(FontFamily.GenericSansSerif, templateExt.DopTextFontWeight.Value * mapSize, FontStyle.Bold);
                    graf.DrawString(templateExt.DopText + " " + percentDec.ToString() + "%", fontData, blackBrush, templateExt.DopTextX.Value * mapSize, templateExt.DopTextY.Value * mapSize);
                }
                if (templateExt.SlovoObichCenaFontWeight.Value != 0)
                {
                    Font fontData = new Font(FontFamily.GenericSansSerif, templateExt.SlovoObichCenaFontWeight.Value * mapSize, FontStyle.Regular);
                    graf.DrawString(templateExt.SlovoObichCena, fontData, blackBrush, templateExt.SlovoObichCenaX.Value * mapSize, templateExt.SlovoObichCenaY.Value * mapSize);
                }
            }
            //- 06052014


            MemoryStream memStream = new MemoryStream();
            context.Response.ContentType = "image/png";
            image.Save(memStream, ImageFormat.Png);

            memStream.WriteTo(context.Response.OutputStream);


            graf.Dispose();
            image.Dispose();
            backImage.Dispose();
        }


        public enum UnitTypes
        {
            Cm,
            In,
            Mm,
            Px
        }

        public struct Unit
        {
            public Unit(double value, UnitTypes type)
            {
                this._value = value;

                this._type = type;
            }

            private double _value;
            public double Value
            {
                get { return _value; }
                set { _value = value; }
            }

            private UnitTypes _type;
            public UnitTypes Type
            {
                get { return _type; }
                set
                {
                    this.Value = this.To(value).Value;

                    _type = value;
                }
            }

            public double GetPixelPer(UnitTypes unitType)
            {
                switch (unitType)
                {
                    case UnitTypes.Cm:

                        return this.GetPixelPer(UnitTypes.In) / 2.54F;
                    case UnitTypes.In:

                        return 96;
                    case UnitTypes.Mm:

                        return this.GetPixelPer(UnitTypes.Cm) / 10;
                    default:

                        return 1;
                }
            }

            public Unit To(UnitTypes unitType)
            {
                return new Unit((this.Value * this.GetPixelPer(this.Type)) / this.GetPixelPer(unitType), unitType);
            }

            public static Unit operator +(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) + b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static Unit operator -(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) - b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static Unit operator *(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) * b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static Unit operator /(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) / b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static implicit operator double(Unit u)
            {
                return u.To(UnitTypes.Px);
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", this.Value.ToString(), this.Type.ToString());
            }
        }


        public Image ResizeImg(Image b, int nWidth, int nHeight)
        {
            Image result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(b, 0, 0, nWidth, nHeight);
                g.Dispose();
            }
            return result;
        }

        private Image cropImage(Image image, Rectangle imageRectangle)
        {
            Bitmap bitmap = new Bitmap(image);
            Bitmap cropedBitmap = bitmap.Clone(imageRectangle, bitmap.PixelFormat);
            return (Image)(cropedBitmap);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}