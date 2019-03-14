<%@ Page Title="Функции загрузки весов" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="WLoads.aspx.cs" Inherits="Saalut.WLoads" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <hgroup class="title">
        <h1>
            <%: Page.Title %>.</h1>
    </hgroup>
    <article>




        <p>
            Загрузка весов по отделам QLOAD:<br />
            Номера весов:<br />
            <asp:PlaceHolder ID="WeightsPlaceHolder1" runat="server"></asp:PlaceHolder>
            <br />
            <asp:TextBox ID="WLMessageTextBox1" runat="server" Enabled="False" ReadOnly="True" Rows="10" TextMode="MultiLine" Width="100%"></asp:TextBox>
            <br />
            <asp:Button ID="LoadQWeighButton1" runat="server"
                Text="Загрузить весы" OnClick="LoadQWeighButton1_Click" />
            <br />
        </p>

        <br />
        <br />
        <p>
            Инициализация:<br />
            <asp:Button ID="SelectTestButton1" runat="server" Text="Инициализация БД"
                OnClick="SelectTestButton1_Click" />
            <br />
        </p>

        <br />
        <br />

        <p>
            Быстрая загрузка материала из УКМ:<br />

            Артикул:<asp:TextBox ID="ArtTextBox1" runat="server"></asp:TextBox>
            <asp:Button ID="LoadArtButton2" runat="server" Text="Загрузить" OnClick="LoadArtButton2_Click" />

            <br />
        </p>

        <br />
        <br />
        <p>
            Старая версия загрузки для Москвы (SIS2002):<br />
            <asp:Button ID="SendAllToWEButton1" runat="server" Text="Отправить все товары на весы."
                OnClick="SendAllToWEButton1_Click" />
            <br />
        </p>

    </article>
    <aside>
        <h3>Отчеты</h3>
        <p>
            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="WeightRep.aspx">Отчет по наличию весового товара в весах.</asp:HyperLink>
        </p>
        <br />
    </aside>
</asp:Content>
