<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TestPage2.aspx.cs" Inherits="Saalut.TestPage2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Button ID="Button1" runat="server" Text="Button" OnClick="Button1_Click" />
    <br /><br /><br />
    Артикул:<asp:TextBox ID="ArtTextBox1" runat="server"></asp:TextBox>
    <asp:Button ID="LoadArtButton2" runat="server" Text="Загрузить" OnClick="LoadArtButton2_Click" />
</asp:Content>
