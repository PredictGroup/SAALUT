<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TestPage.aspx.cs" Inherits="Saalut.TestPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Button ID="SelectTestButton1" runat="server" Text="Инициализация БД"
        OnClick="SelectTestButton1_Click" />

    <asp:Button ID="InitMatWeightButton1" runat="server" Text="Инициализация привязок материлов к весам"
        OnClick="InitMatWeightButton1_Click" />

    <asp:Button ID="ExportToUKMButton1" runat="server" Text="Выгрузить на Весы"
        OnClick="ExportToUKMButton1_Click" />

    <br />
    <br />
    <br />
    Бытрое обновление товара. Артикул:<asp:TextBox ID="ArtTextBox1" runat="server"></asp:TextBox>
    <asp:Button ID="LoadArtButton2" runat="server" Text="Загрузить" OnClick="LoadArtButton2_Click" />

    <br />
    <br />
    <br />
    Номера весов:<br />
    <asp:PlaceHolder ID="WeightsPlaceHolder1" runat="server"></asp:PlaceHolder>

    <asp:Button ID="LoadQWeighButton1" runat="server"
        Text="Загрузить весы" OnClick="LoadQWeighButton1_Click" />

    <br />
    <br />
    <br />

    <br />
    <br />
    <br />

    <br />
    <br />
    <br />


    <asp:TextBox ID="MessageTextBox1" runat="server" Rows="15" Enabled="false" TextMode="MultiLine" Width="95%"></asp:TextBox>

    <br />

</asp:Content>
