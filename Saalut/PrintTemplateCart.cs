using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;


[Serializable()]
public class PrintTemplateCartItem
{
    private int jourLineID;
    private int templateID;
    private string proizvoditel;
    private string goodName;

    public int TemplateID
    {
        get { return templateID; }
    }

    public int JourLineID
    {
        get { return jourLineID; }
    }

    public string Proizvoditel
    {
        get { return proizvoditel; }
    }

    public string GoodName
    {
        get { return goodName; }
    }

    public PrintTemplateCartItem(int JourLineID, int TemplateID, string Proizvoditel, string GoodName)
    {
        this.jourLineID = JourLineID;
        this.templateID = TemplateID;
        this.proizvoditel = Proizvoditel;
        this.goodName = GoodName;
    }
}

[Serializable()]
public class PrintTemplateCart : CollectionBase
{
    public PrintTemplateCartItem this[int index]
    {
        get { return ((PrintTemplateCartItem)List[index]); }
        set { List[index] = value; }
    }
    public int Add(PrintTemplateCartItem value)
    {
        return (List.Add(value));
    }
    public int IndexOf(PrintTemplateCartItem value)
    {
        return (List.IndexOf(value));
    }
    public void Insert(int index, PrintTemplateCartItem value)
    {
        List.Insert(index, value);
    }
    public void Remove(PrintTemplateCartItem value)
    {
        List.Remove(value);
    }
    public bool Contains(PrintTemplateCartItem value)
    {
        return (List.Contains(value));
    }
}
