using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Security;
using System.Web.Services.Protocols;
using System.Globalization;
using System.Net;
using System.IO;
using System.Web.Configuration;

namespace Saalut.Services
{
    /// <summary>
    /// Summary description for SaalutUtilService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SaalutUtilService : System.Web.Services.WebService
    {

        [WebMethod(Description = "Инициализация начальных данных БД Saalut Express."), SoapDocumentMethod(OneWay = true)]
        public void InitialSetsDBSaalut()
        {
            string ret = "";
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            ret = ukm.InitialDB();
        }

        [WebMethod(Description = "Загрузка товаров и групп в БД Saalut Express."), SoapDocumentMethod(OneWay = true)]
        public void NightLoadsToDB()
        {
            string ret = "";
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            ret = ukm.NightLoadsToDB();
        }

        [WebMethod(Description = "Загрузка журналов изменения цен УКМ."), SoapDocumentMethod(OneWay = true)]
        public void ReadNewDataFromUKM()
        {
            string ret = "";
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            ret = ukm.ReadNewData();
        }

        [WebMethod(Description = "Загрузка товаров в БД Saalut Express."), SoapDocumentMethod(OneWay = true)]
        public void UKM_Quick_LoadsToDB()
        {
            string ret = "";
            UKMDataBaseConnects ukm = new UKMDataBaseConnects();
            ret = ukm.UKM_Quick_LoadsToDB();
        }


        [WebMethod(Description = "Час загрузки данных ночью и выгрузки на весы.")]
        public string LoadHour()
        {
            string ret = WebConfigurationManager.AppSettings["Export_Night_To_Weight"];
            return ret;
        }

    }
}
