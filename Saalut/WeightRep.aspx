<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="WeightRep.aspx.cs" Inherits="Saalut.WeightRep" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Отчет о наличии ассортимента в весах</h3>
    <br />
    Указать артикул или штрихкод или PLU:<br />
    <asp:TextBox ID="ForFindTextBox1" runat="server"></asp:TextBox>
    <br />
    <br />
    <asp:Button ID="SubmitButton1" runat="server" Text="Сформировать отчет" OnClick="SubmitButton1_Click" />
    <br />
    <br />

    <asp:Label ID="ErrorLabel1" runat="server" Text=""></asp:Label>
    <br />
    <asp:Literal ID="OutInfoLiteral1" runat="server"></asp:Literal>

</asp:Content>
