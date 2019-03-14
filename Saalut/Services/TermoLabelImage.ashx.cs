using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace Saalut.Services
{
    /// <summary>
    /// Summary description for TermoLabelImage
    /// </summary>
    public class TermoLabelImage : IHttpHandler
    {

        int mapSize = 1;

        int fontWeight = 8;
        int SimvolovVStroke = 17;
        int otstupVMM = 3;
        int GoodNameX = 4;
        int GoodNameY = 1;
        int PriceRubX = 35;
        int PriceRubY = 25;
        int BarcodeX = 0;
        int BarcodeY = 34;


        public void ProcessRequest(HttpContext context)
        {
            string goodID = (context.Request.QueryString["GID"]).ToUpper();
            
            SaalutDataClasses1DataContext dataContext = new SaalutDataClasses1DataContext();

            int gID = 0;
            Int32.TryParse(goodID, out gID);


            var good = (from g in dataContext.Goods
                        where g.ID == gID
                        select g).FirstOrDefault();
            if (good == null)
                return;

            // settings
            var settings = (from s in dataContext.Settings
                            select s).FirstOrDefault();


            Unit a = new Unit(settings.PrintTermoLabelShirinaMM.Value, UnitTypes.Mm);
            int wImage = Int32.Parse(Math.Round(a.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки
            Unit b = new Unit(settings.PrintTermoLabelVisotaMM.Value, UnitTypes.Mm);
            int hImage = Int32.Parse(Math.Round(b.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки


            wImage = wImage * mapSize; // увеличим размерчик для улучшения качества
            hImage = hImage * mapSize;

            Bitmap image = new Bitmap(wImage, hImage);
            Graphics graf = Graphics.FromImage(image);

            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush blackBrush = new SolidBrush(Color.Black);

            graf.FillRectangle(whiteBrush, 0, 0, wImage, hImage);


            // Информация этикетки

            string descr = good.Name + " Арт.:" + good.Articul;
            StringBuilder str = new StringBuilder();

            descr = descr.Replace("\r\n", "\n");
            descr = descr.Replace("\r", "\n");
            descr = descr.Replace("\t", " ");

            int length = descr.Length;
            int from = 0;

            while (length >= SimvolovVStroke) //количество символов в строке
            {
                str.Append(descr.Substring(from, SimvolovVStroke) + "\n");
                from += SimvolovVStroke;
                length -= SimvolovVStroke;
            }
            if (length < SimvolovVStroke)
                str.Append(descr.Substring(from, length));

            string[] textParagraphs = str.ToString().Split('\n');


            Font fontGoodName = new Font(FontFamily.GenericSansSerif, fontWeight * mapSize, FontStyle.Regular);
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
                        Unit o = new Unit(otstupVMM, UnitTypes.Mm);
                        otstPx = Int32.Parse(Math.Round(o.To(UnitTypes.Px).Value, 0).ToString()); //длинна картинки
                    }
                    graf.DrawString(strText.Trim(), fontGoodName, blackBrush, GoodNameX * mapSize, (GoodNameY + (i * otstPx)) * mapSize, sf);

                }
                i++;
            }

            if (settings.PrintTermoLabelPrice.Value)
            {
                string priceVal = "нет цены";
                var price = (from p in dataContext.Prices
                             where p.GoodID == good.ID
                             && p.Active == true
                             select p).FirstOrDefault();
                if (price != null)
                    priceVal = price.Price1.Value.ToString();


                Font fontPriceRub = new Font(FontFamily.GenericSansSerif, fontWeight * mapSize, FontStyle.Regular);
                graf.DrawString(priceVal + " руб.", fontPriceRub, blackBrush, PriceRubX * mapSize, PriceRubY * mapSize);
            }

            //---


            Image img = image.GetThumbnailImage(wImage / mapSize, hImage / mapSize, null, (new System.IntPtr(0)));

            Graphics graf2 = Graphics.FromImage(img);

            string barcode = good.Barcode;
            if (barcode.Length == 13)
                barcode = barcode.Substring(0, 12);


            if (barcode.Length < 12)
            {
                int ii = 12 - barcode.Length;
                for (int i2 = 1; i2 <= ii; i2++)
                    barcode += "0";
            }

            BarcodeEAN13 encoderEAN13 = new BarcodeEAN13();
            Image barcImg = encoderEAN13.Encode(barcode);
            //
            Rectangle nr = new Rectangle(0, barcImg.Height / 2, barcImg.Width, barcImg.Height / 3);
            //graf2.DrawImage(barcImg, new Point(BarcodeX, BarcodeY));
            graf2.DrawImage(this.cropImage(barcImg, nr), new Point(BarcodeX, BarcodeY));

            MemoryStream memStream = new MemoryStream();
            context.Response.ContentType = "image/png";
            img.Save(memStream, ImageFormat.Png);

            memStream.WriteTo(context.Response.OutputStream);


            graf.Dispose();
            image.Dispose();
            barcImg.Dispose();
            img.Dispose();
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