<%@ Page Title="Печать термоэтикетки" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="TermoLabelToLPT.aspx.cs" Inherits="Saalut.TermoLabelToLPT" %>

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
            Проверьте принтер, данные отправлены на печать.<br />
            <asp:Button ID="BackButton1" runat="server" Text="Вернуться" OnClick="BackButton1_Click" />
        </p>
    </article>
    <aside>
        <h3>Инструкция</h3>
        <p>
            Данные были отправлены на печать, если на термопринтере ничего не произошло, необходимо
            привязать порт Lpt указанный в настройках (DB Settings) или проверить этикетку,
            которая находится на сервере в папке TRMLabels.
        </p>
    </aside>
</asp:Content>
