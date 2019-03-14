<%@ Page Title="Главная" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Saalut._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1>
                    <%: Page.Title %>.</h1>
                <h2>SAP АЛМИ Утилиты - используется для печати ценников и загрузки весов.</h2>
            </hgroup>
            <p>

                Для получения документации нажмите на ссылку: <a href="Saalut.pdf" title="Документация по работе с программой">Saalut.pdf</a>
                <a href="ЗагрузкаЦенУКМ-1.doc" title="Документация по работе с УКМ">ЗагрузкаЦенУКМ-1.doc</a>
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <section class="features">
        <section class="feature">
            <a href="NewJours.aspx">
                <img runat="server" src="~/Images/aspNetHome.png" alt="" style="border: 0;" /></a>
            <h3>Новые цены из SAP</h3>
            <p>
                Печать ценников по журналу изменения цен из УКМ.
            </p>
        </section>
        <section class="feature">
            <a href="ByFind.aspx">
                <img runat="server" src="~/Images/NuGetGallery.png" alt="" style="border: 0;" /></a>
            <h3>Ценники по списку</h3>
            <p>
                Печать ценников по списку из SAP.
            </p>
        </section>
        <section class="feature">
            <a href="PriceJours.aspx">
                <img id="Img1" runat="server" src="~/Images/NuGetGallery.png" alt="" style="border: 0;" /></a>
            <h3>Журналы цен</h3>
            <p>
                Печать ценников по журналу приходных документов или по журналу из терминала сбора
                данных (ТСД).
            </p>
        </section>
        <section class="feature">
            <a href="WLoads.aspx">
                <img runat="server" src="~/Images/findHosting.png" alt="" style="border: 0;" /></a>
            <h3>Загрузка весов</h3>
            <p>
                Загрузка весов ценами, составом номенклатуры из SAP.
            </p>
        </section>
    </section>
    <div class="float-left">
        <h3>В случае наличия замечаний по работе программы, просьба обращаться:</h3>
        <ol class="round">
            <li class="one">
                <h5>ИТ департамент:</h5>
                Телефон: ******.</li>
            <li class="two">
                <h5>ИТ департамент:</h5>
                Телефон: ******.</li>
            <li class="three">
                <h5>По электронной почте разработчику:</h5>
                *********. </li>
        </ol>
    </div>
</asp:Content>
