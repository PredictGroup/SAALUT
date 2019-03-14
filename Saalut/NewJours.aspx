<%@ Page Title="Печать ценников по журналу изменения цен УКМ" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="NewJours.aspx.cs" Inherits="Saalut.NewJours" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
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
        <h4>
            <asp:Label ID="MessageLabel1" runat="server" Text=""></asp:Label>
        </h4>
        <p>
            <asp:GridView ID="NewJoursGridView1" runat="server" AllowPaging="True" AllowSorting="True"
                AutoGenerateColumns="False" DataKeyNames="ID" DataSourceID="NewJoursLinqDataSource1"
                Width="90%" OnRowDataBound="NewJoursGridView1_RowDataBound" OnSelectedIndexChanged="NewJoursGridView1_SelectedIndexChanged"
                PageSize="25">
                <Columns>
                    <asp:TemplateField ShowHeader="False">
                        <ItemTemplate>
                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Select"
                                Text="Удалить"></asp:LinkButton>
                            <ajaxToolkit:ConfirmButtonExtender ID="cbe" runat="server" TargetControlID="LinkButton1"
                                ConfirmText="Удалить журнал?" />
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True"
                        SortExpression="ID" Visible="False" />
                    <asp:BoundField DataField="Order_no" HeaderText="Журнал УКМ" SortExpression="Order_no">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:BoundField DataField="ByDocument" HeaderText="ByDocument" SortExpression="ByDocument"
                        Visible="False" />
                    <asp:CheckBoxField DataField="FromSAP" HeaderText="FromSAP" SortExpression="FromSAP"
                        Visible="False" />
                    <asp:CheckBoxField DataField="FromTerminal" HeaderText="FromTerminal" SortExpression="FromTerminal"
                        Visible="False" />
                    <asp:BoundField DataField="TimeStamp" HeaderText="Изменен" SortExpression="TimeStamp">
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:BoundField>
                    <asp:CheckBoxField DataField="InUse" HeaderText="InUse" SortExpression="InUse" Visible="False" />
                    <asp:CheckBoxField DataField="Active" HeaderText="Active" SortExpression="Active"
                        Visible="False" />
                    <asp:TemplateField>
                        <ItemTemplate>
                            <asp:HyperLink ID="GoToLinesHyperLink1" runat="server" Text="Открыть"></asp:HyperLink>
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Center" />
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:LinqDataSource ID="NewJoursLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EntityTypeName="" TableName="PriceChangeJours" Where="Active == @Active &amp;&amp; Change_log_id_UKM != @Change_log_id_UKM"
                OrderBy="TimeStamp desc">
                <WhereParameters>
                    <asp:Parameter DefaultValue="True" Name="Active" Type="Boolean" />
                    <asp:Parameter DefaultValue="0" Name="Change_log_id_UKM" Type="Int32" />
                </WhereParameters>
            </asp:LinqDataSource>
        </p>
    </article>
    <aside>
        <h3>Действия</h3>
        <p>
            <asp:Button ID="LoadNewJoursButton1" runat="server" Text="Загрузить журналы" OnClick="LoadNewJoursButton1_Click" />
        </p>
        <br />
        <h3>Инструкция</h3>
        <p>
            Перечислены активные журналы, если здесь остается журнал распечатанный и отправленный
                    на весы, следовательно в нем не все позиции отпечатаны и утверждены.
        </p>
    </aside>

</asp:Content>
