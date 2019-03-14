<%@ Page Title="Остатки в открытых сменах (УКМ и SAP ERP)" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UKM_SAPERP_OpenStocks.aspx.cs" Inherits="Saalut.UKM_SAPERP_OpenStocks" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Остатки в открытых сменах</h3>
    <br />
    <asp:Wizard runat="server" ID="Wizard1" EnableTheming="False" DisplayCancelButton="false">
        <WizardSteps>
            <asp:WizardStep ID="Init" EnableTheming="False" StepType="Auto" runat="server" AllowReturn="False" Title=" ">
                Дата в формате:<br />
                <asp:TextBox ID="txbOnDate" runat="server"></asp:TextBox>
                <br />
                <br />
                Материалы:<br />
                <asp:TextBox ID="txbMaters" runat="server" Rows="10" TextMode="MultiLine"></asp:TextBox>
                <br />
                <asp:Button ID="SubmitButton1" runat="server" Text="Сформировать отчет" OnClick="SubmitButton1_Click" />
                <br />
                <br />
                * список материалов в столбец по одному.
                <br />
                <br />
                <br />
                <asp:CheckBox ID="cbxGetData" Text="Получать данные повторно?" Checked="true" runat="server" />
            </asp:WizardStep>
            <asp:WizardStep ID="Report" AllowReturn="True" EnableTheming="False" runat="server" StepType="Auto" Title=" ">
                <br />
                <asp:Label ID="ErrorLabel1" ForeColor="Red" runat="server" Text=""></asp:Label>
                <br />
                <asp:LinkButton ID="RefreshLinkButton1" runat="server" OnClick="RefreshLinkButton1_Click">Сформировать повторно</asp:LinkButton>
                <br />
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
