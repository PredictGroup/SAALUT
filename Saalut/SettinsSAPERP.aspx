<%@ Page Title="Настройка соединения с SAP ERP " Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SettinsSAPERP.aspx.cs" Inherits="Saalut.SettinsSAPERP" %>

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
            <asp:DetailsView ID="SAPInfoDetailsView1" runat="server" Height="50px" Width="100%"
                AutoGenerateRows="False" DataKeyNames="ID" DataSourceID="SAPInfoLinqDataSource1">
                <Fields>
                    <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True"
                        SortExpression="ID" Visible="False" />
                    <asp:TemplateField HeaderText="Апп сервер" SortExpression="MessageServerHost">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("MessageServerHost") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("MessageServerHost") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("MessageServerHost") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Logon Group" SortExpression="LogonGroup">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("LogonGroup") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox2" runat="server" Text='<%# Bind("LogonGroup") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("LogonGroup") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="ИД системы" SortExpression="SystemID">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("SystemID") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("SystemID") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label3" runat="server" Text='<%# Bind("SystemID") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Номер системы" SortExpression="SystemNumber">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("SystemNumber") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox4" runat="server" Text='<%# Bind("SystemNumber") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label4" runat="server" Text='<%# Bind("SystemNumber") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Мандант" SortExpression="Client">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox5" runat="server" Text='<%# Bind("Client") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox5" runat="server" Text='<%# Bind("Client") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label5" runat="server" Text='<%# Bind("Client") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="SAP Router" SortExpression="SAPRouter">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox6" runat="server" Text='<%# Bind("SAPRouter") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox6" runat="server" Text='<%# Bind("SAPRouter") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label6" runat="server" Text='<%# Bind("SAPRouter") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Пользователь" SortExpression="SAPUser">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox7" runat="server" Text='<%# Bind("SAPUser") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox7" runat="server" Text='<%# Bind("SAPUser") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label7" runat="server" Text='<%# Bind("SAPUser") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Пароль" SortExpression="SAPPassword">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox8" runat="server" Text='<%# Bind("SAPPassword") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox8" runat="server" Text='<%# Bind("SAPPassword") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label8" runat="server" Text='<%# Bind("SAPPassword") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Завод магазина в SAP" SortExpression="Werk">
                        <EditItemTemplate>
                            <asp:TextBox ID="TextBox9" runat="server" Text='<%# Bind("Werk") %>' Width="90%"></asp:TextBox>
                        </EditItemTemplate>
                        <InsertItemTemplate>
                            <asp:TextBox ID="TextBox9" runat="server" Text='<%# Bind("Werk") %>' Width="90%"></asp:TextBox>
                        </InsertItemTemplate>
                        <ItemTemplate>
                            <asp:Label ID="Label8" runat="server" Text='<%# Bind("Werk") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField CancelText="Отмена" EditText="Изменить" ShowEditButton="True" UpdateText="Обновить" />
                </Fields>
            </asp:DetailsView>
            <asp:LinqDataSource ID="SAPInfoLinqDataSource1" runat="server" ContextTypeName="Saalut.SaalutDataClasses1DataContext"
                EnableUpdate="True" EntityTypeName="" TableName="SettingsSAPERPTbls">
            </asp:LinqDataSource>
        </p>
    </article>
    <aside>
        <h3>Инструкция</h3>
        <p>
            Указанные настройки используются для соединения с сервером приложений SAP ERP.
        </p>
        <br />
    </aside>
</asp:Content>
