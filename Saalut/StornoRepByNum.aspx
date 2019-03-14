<%@ Page Title="Отчет об аннуляциях и сторно на кассах по номеру смены" Language="C#"
    MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StornoRepByNum.aspx.cs"
    Inherits="Saalut.StornoRepByNum" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Отчет об аннуляциях и сторно на кассах по номеру смены</h3>
    <br />
    <asp:Wizard runat="server" ID="Wizard1" EnableTheming="False" DisplayCancelButton="false">
        <WizardSteps>
            <asp:WizardStep ID="Init" EnableTheming="False" StepType="Auto" runat="server" AllowReturn="False"
                Title=" ">
                <asp:Button ID="SubmitButton1" runat="server" Text="Сформировать отчет" OnClick="SubmitButton1_Click" />
                <br />
                <br />

                <table style="width: 100%;">
                    <tr>
                        <td>Номер смены:</td>
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
            </asp:WizardStep>
            <asp:WizardStep ID="Report" AllowReturn="True" EnableTheming="False" runat="server"
                StepType="Auto" Title=" ">
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
