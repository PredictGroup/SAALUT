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
public class JourCartItem
{
    private int jourLineID;

    public int JourLineID
    {
        get { return jourLineID; }
    }

    public JourCartItem(int JourLineID)
    {
        this.jourLineID = JourLineID;
    }
}

[Serializable()]
public class JourCart : CollectionBase
{
    public JourCartItem this[int index]
    {
        get { return ((JourCartItem)List[index]); }
        set { List[index] = value; }
    }
    public int Add(JourCartItem value)
    {
        return (List.Add(value));
    }
    public int IndexOf(JourCartItem value)
    {
        return (List.IndexOf(value));
    }
    public void Insert(int index, JourCartItem value)
    {
        List.Insert(index, value);
    }
    public void Remove(JourCartItem value)
    {
        List.Remove(value);
    }
    public bool Contains(JourCartItem value)
    {
        return (List.Contains(value));
    }
}
