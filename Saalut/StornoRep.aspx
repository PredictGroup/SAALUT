<%@ Page Title="Отчет об аннуляциях и сторно на кассах в открытой смене" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StornoRep.aspx.cs" Inherits="Saalut.StornoRep" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Отчет об аннуляциях и сторно на кассах в открытой смене</h3>
    <br />
    <asp:Wizard runat="server" ID="Wizard1" EnableTheming="False" DisplayCancelButton="false">
        <WizardSteps>
            <asp:WizardStep ID="Init" EnableTheming="False" StepType="Auto" runat="server" AllowReturn="False" Title=" ">
                <asp:Button ID="SubmitButton1" runat="server" Text="Сформировать отчет" OnClick="SubmitButton1_Click" />
                <br />
            </asp:WizardStep>
            <asp:WizardStep ID="Report" AllowReturn="True" EnableTheming="False" runat="server" StepType="Auto" Title=" ">
                <br />
                <asp:Label ID="ErrorLabel1" runat="server" Text=""></asp:Label>
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
