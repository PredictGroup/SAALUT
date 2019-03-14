<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Reports.aspx.cs" Inherits="Saalut.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <h3>Отчеты</h3>
    <br />
    <asp:HyperLink ID="HyperLink7" runat="server" NavigateUrl="~/UKM_SAPERP_CheckPrices.aspx">Сверка цен УКМ и SAP ERP.</asp:HyperLink>
    <br />
    <br />
    <asp:HyperLink ID="HyperLink8" runat="server" NavigateUrl="~/UKM_SAPERP_OpenStocks.aspx">Остатки в открытых сменах</asp:HyperLink>
    <br />
    <br />
    <hr />
    <br />
    <asp:HyperLink ID="HyperLink5" runat="server" NavigateUrl="~/DljaRevizorovRep.aspx">Отчет для ревизоров. Продажи по кассе и смене.</asp:HyperLink>
    <br />
    <br />
    <asp:HyperLink ID="HyperLink6" runat="server" NavigateUrl="~/DljaRevizorovOpenRep.aspx">Отчет для ревизоров. Продажи в открытой смене.</asp:HyperLink>
    <br />
    <br />
    <hr />
    <br />
    <asp:HyperLink ID="HyperLink2" runat="server" NavigateUrl="~/CigaretteRep.aspx">Отчет по продажам по группе в открытой смене</asp:HyperLink>
    <br />
    <br />
    <asp:HyperLink ID="HyperLink4" runat="server" NavigateUrl="~/CigaretteRepAny.aspx">Отчет по продажам по группе по смене и кассе</asp:HyperLink>
    <br />
    <br />
    <br />
    <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/StornoRep.aspx">Отчет об аннуляциях и сторно на кассах в открытой смене</asp:HyperLink>
    <br />
    <br />
    <asp:HyperLink ID="HyperLink3" runat="server" NavigateUrl="~/StornoRepByNum.aspx">Отчет об аннуляциях и сторно на кассах по номеру смены</asp:HyperLink>
    <br />
    <br />
    <br />
    <br />
    <br />

    <br />
    <br />
    <br />
</asp:Content>
