<%@ Page Title="Отчет по продажам по группе в открытой смене" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="CigaretteRep.aspx.cs" Inherits="Saalut.CigaretteRep" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Отчет по продажам по группе в открытой смене</h3>
    <br />
    <asp:Wizard runat="server" ID="Wizard1" EnableTheming="False" DisplayCancelButton="false">
        <WizardSteps>
            <asp:WizardStep ID="Init" EnableTheming="False" StepType="Auto" runat="server" AllowReturn="False" Title=" ">
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
