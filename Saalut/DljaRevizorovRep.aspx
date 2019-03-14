<%@ Page Title="Для ревизоров. Отчет по продажам по группе по смене и кассе" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DljaRevizorovRep.aspx.cs" Inherits="Saalut.DljaRevizorovRep" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Отчет по продажам по группе по смене и кассе</h3>
    <br />
    <asp:Wizard runat="server" ID="Wizard1" EnableTheming="False" DisplayCancelButton="false">
        <WizardSteps>
            <asp:WizardStep ID="Init" EnableTheming="False" StepType="Auto" runat="server" AllowReturn="False" Title=" ">
                <table style="width: 100%;">
                    <tr>
                        <td>Номер смены (как указано в УКМ):</td>
                        <td>
                            <asp:TextBox ID="NumTextBox1" runat="server"></asp:TextBox></td>
                    </tr>
                    <tr>
                        <td>Номер кассы:</td>
                        <td>
                            <asp:TextBox ID="CashTextBox1" runat="server"></asp:TextBox></td>
                    </tr>
                </table>
                <br />
                <br />
                <asp:Button ID="PreRepButton1" runat="server" Text="Группы" OnClick="PreRepButton1_Click" Visible="false" />
                <asp:Button ID="SubmitButton1" runat="server" Text="Сформировать отчет" OnClick="SubmitButton1_Click" />
                <br />
                <br />
                <asp:ListBox ID="CheckedListBox1" runat="server" Width="300px" Rows="15" DataTextField="GroupName"
                    DataValueField="ID"></asp:ListBox>
                <asp:Button ID="AddButton1" runat="server" Text="<" OnClick="AddButton1_Click" />
                <asp:Button ID="DeleteButton1" runat="server" Text=">" OnClick="DeleteButton1_Click" />
                <asp:ListBox ID="SelectListBox1" runat="server" Width="300px" Rows="15" DataTextField="GroupName"
                    DataValueField="ID"></asp:ListBox>
            </asp:WizardStep>
            <asp:WizardStep ID="Report" AllowReturn="True" EnableTheming="False" runat="server" StepType="Auto" Title=" ">
                <br />
                <asp:Label ID="ErrorLabel1" runat="server" Text=""></asp:Label>
                <br />
                <table style="width: 100%;">
                    <tr>
                        <td>Номер смены:</td>
                        <td>
                            <asp:Label ID="NumLabel1" runat="server" Text=""></asp:Label></td>
                    </tr>
                    <tr>
                        <td>Номер кассы:</td>
                        <td>
                            <asp:Label ID="CashLabel1" runat="server" Text=""></asp:Label></td>
                    </tr>
                </table>
                <br />
                <asp:PlaceHolder ID="ReportPlaceHolder1" runat="server"></asp:PlaceHolder>
                <br />
                <asp:Label ID="DebugLabel1" runat="server" Text=""></asp:Label>
            </asp:WizardStep>
        </WizardSteps>
        <StepNavigationTemplate>
        </StepNavigationTemplate>
        <StartNavigationTemplate>
        </StartNavigationTemplate>
        <FinishNavigationTemplate>
        </FinishNavigationTemplate>
    </asp:Wizard>
</asp:Content>
