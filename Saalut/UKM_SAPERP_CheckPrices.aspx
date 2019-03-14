<%@ Page Title="Сверка цен УКМ и SAP ERP." Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UKM_SAPERP_CheckPrices.aspx.cs" Inherits="Saalut.UKM_SAPERP_CheckPrices" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Сверка цен УКМ и SAP ERP</h3>
    <br />
    <asp:Wizard runat="server" ID="Wizard1" EnableTheming="False" DisplayCancelButton="false">
        <WizardSteps>
            <asp:WizardStep ID="Init" EnableTheming="False" StepType="Auto" runat="server" AllowReturn="False" Title=" ">
                <asp:Button ID="SubmitButton1" runat="server" Text="Сформировать отчет" OnClick="SubmitButton1_Click" />
                <br />
                <br />
            </asp:WizardStep>
            <asp:WizardStep ID="Report" AllowReturn="True" EnableTheming="False" runat="server" StepType="Auto" Title=" ">
                <br />
                <asp:Label ID="ErrorLabel1" runat="server" Text=""></asp:Label>
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

