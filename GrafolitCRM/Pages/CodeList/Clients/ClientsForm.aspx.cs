﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AnalizaProdaje.Common;
using DatabaseWebService.Models.Client;
using DevExpress.Web;
using System.Data;
using Newtonsoft.Json;
using AnalizaProdaje.Domain.Helpers;
using System.Reflection;
using DatabaseWebService.Models;
using AnalizaProdaje.UserControls;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Web.Configuration;
using System.Drawing;
using DatabaseWebService.Models.Event;
namespace AnalizaProdaje.Pages.CodeList.Clients
{
    public partial class ClientsForm : ServerMasterPage
    {
        ClientFullModel model = null;
        int clientID = -1;
        int action = -1;
        int planFocusedRow = 0;
        int contactPersonFocusedRow = 0;
        int chartsInRow = 0;

        string activeTabName = "";

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated) RedirectHome();

            if (Request.QueryString[Enums.QueryStringName.action.ToString()] == null)
            {
                RedirectHome();
            }

            action = CommonMethods.ParseInt(Request.QueryString[Enums.QueryStringName.action.ToString()].ToString());
            clientID = CommonMethods.ParseInt(Request.QueryString[Enums.QueryStringName.recordId.ToString()].ToString());



            //if user's previous page was eventsForm then we check if there is an query string for setting active tab-s
            if (SessionHasValue(Enums.CommonSession.activeTab))
            {
                activeTabName = GetValueFromSession(Enums.CommonSession.activeTab).ToString();
                ClientPageControl.ActiveTabIndex = ClientPageControl.TabPages.IndexOfName(activeTabName);
                RemoveSession(Enums.CommonSession.activeTab);
            }


            //if (SessionHasValue(Enums.ClientSession.PlanPopUpID))
            //   planFocusedRow = CommonMethods.ParseInt(GetValueFromSession(Enums.ClientSession.PlanPopUpID).ToString());

            //if (SessionHasValue(Enums.ClientSession.ContactPersonPopUp))
            //  contactPersonFocusedRow = CommonMethods.ParseInt(GetValueFromSession(Enums.ClientSession.ContactPersonPopUp).ToString());

            //if(!Request.IsAuthenticated)
            //    this.Master.ReSignIn = true;

            this.Master.DisableNavBar = true;

            ClientPageControl.TabPages.FindByName("Plan").Enabled = false;
            ClientPageControl.TabPages.FindByName("Plan").ClientVisible = false;

            if (Request.UrlReferrer != null && Request.UrlReferrer.AbsolutePath.Contains("ClientsChartsDetail.aspx"))
            { //if usere has clicked back button on browser than sessions have not deleted
                ClearSessions();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                Initialize();
                if (action == (int)Enums.UserAction.Edit || action == (int)Enums.UserAction.Delete)
                {
                    if (clientID > 0)
                    {
                        if (GetClientDataProviderInstance().GetFullModelFromClientModel() != null)
                            model = GetClientDataProviderInstance().GetFullModelFromClientModel();
                        else if (PrincipalHelper.IsUserSuperAdmin() || PrincipalHelper.IsUserAdmin())
                            model = CheckModelValidation<ClientFullModel>(GetDatabaseConnectionInstance().GetClient(clientID));
                        else
                        {//this else checks if the signed in user actually have rights to edit this client!
                            model = CheckModelValidation<ClientFullModel>(GetDatabaseConnectionInstance().GetClient(clientID, PrincipalHelper.GetUserPrincipal().ID));
                            if (model == null) RedirectHome();
                        }

                        if (model != null)
                        {
                            GetClientDataProviderInstance().SetClientFullModel(model);
                            FillForm();
                        }
                        //This popup shows if we set the session ShowWarning
                        ShowWarningPopUp("'Trenutna stranka ni shranjena. Če želite urejati kontaktne osebe, grafe, naprave, kategorije je potrebno prvo stranko shraniti!'");
                    }
                }
                else if (action == (int)Enums.UserAction.Add)
                {
                    ComboBoxZaposleniStranke.SelectedIndex = 0;
                }
                InitializeEditDeleteButtons();
                UserActionConfirmBtnUpdate(btnConfirm, action);

                //We use this code if user visit EventsForm page from ClientsForm page tab Events. 
                if (activeTabName.Equals("Events"))
                    ASPxGridViewEvents.DataBind();
            }
            else
            {
                if (model == null && SessionHasValue(Enums.ClientSession.ClientModel))
                    model = (ClientFullModel)GetValueFromSession(Enums.ClientSession.ClientModel);
            }

            if (ClientPageControl.TabPages.FindByName("Charts").IsActive)
            {   //First Tab Charts
                if (SessionHasValue(Enums.ChartSession.GraphCollection))
                {
                    ChartsCallback.Controls.Clear();
                    AddControlsToPanel(GetClientDataProviderInstance().GetGraphBindingList());
                }
                else if (action == (int)Enums.UserAction.Add)
                {
                    ClientPageControl.ActiveTabIndex = ClientPageControl.TabPages.IndexOfName("Basic");
                    ClientPageControl.TabPages.FindByName("Charts").ClientEnabled = false;
                }
            }

            //Assigning focused row to Plan gridview
            if (planFocusedRow > 0)
            {
                ASPxGridViewPlan.FocusedRowIndex = ASPxGridViewPlan.FindVisibleIndexByKeyValue(planFocusedRow);
                ASPxGridViewPlan.ScrollToVisibleIndexOnClient = ASPxGridViewPlan.FindVisibleIndexByKeyValue(planFocusedRow);
            }

            //Assigning focused row to ContactPerson gridview
            if (contactPersonFocusedRow > 0)
            {
                ASPxGridViewContactPerson.FocusedRowIndex = ASPxGridViewContactPerson.FindVisibleIndexByKeyValue(contactPersonFocusedRow);
                ASPxGridViewContactPerson.ScrollToVisibleIndexOnClient = ASPxGridViewContactPerson.FindVisibleIndexByKeyValue(contactPersonFocusedRow);
            }
        }

        private void FillForm()
        {
            txtSifraStranke.Text = model.KodaStranke;
            txtIdStranke.Text = model.idStranka.ToString();
            txtNazivPrvi.Text = model.NazivPrvi;
            txtNazivDrugi.Text = model.NazivDrugi;
            txtNaslov.Text = model.Naslov;
            txtStevPoste.Text = model.StevPoste;
            txtNazivPoste.Text = model.NazivPoste;
            txtEmail.Text = model.Email;
            txtTelefon.Text = model.Telefon;
            txtFax.Text = model.FAX;
            txtInternetniNalov.Text = model.InternetniNalov;
            txtKontaktnaOseba.Text = model.KontaktnaOseba;
            txtRokPlacila.Text = model.RokPlacila;
            txtRangStranke.Text = model.RangStranke;
            ComboBoxZaposleniStranke.SelectedIndex = model.StrankaZaposleni.Count > 0 ? ComboBoxZaposleniStranke.Items.IndexOfValue(model.StrankaZaposleni[0].idOsebe.ToString()) : 0;
            cmbAktivnost.SelectedItem = model.Aktivnost == 1 ? cmbAktivnost.Items.FindByText("DA") : cmbAktivnost.Items.FindByText("NE");
            //cmbAktivnost.SelectedIndex = model.Aktivnost > 0 ? cmbAktivnost.Items.IndexOfValue(CommonMethods.ParseInt(model.Aktivnost)) : 0;

            if (PrincipalHelper.IsUserSalesman() || PrincipalHelper.IsUserUser())
            {
                ComboBoxZaposleniStranke.BackColor = Color.LightGray;
                ComboBoxZaposleniStranke.ReadOnly = true;
                ComboBoxZaposleniStranke.Enabled = false;
            }

            ASPxRoundPanel1.HeaderText = model.NazivPrvi;
        }

        private bool AddOrEditEntityObject(bool add = false)
        {
            if (add)
            {
                model = new ClientFullModel();

                model.idStranka = 0;
                model.ts = DateTime.Now;
                model.tsIDOsebe = PrincipalHelper.GetUserPrincipal().ID;

            }
            else if (model == null && !add)
            {
                model = (ClientFullModel)GetValueFromSession(Enums.ClientSession.ClientModel);
                model.idStranka = CommonMethods.ParseInt(txtIdStranke.Text);
            }

            if (model.StrankaZaposleni == null || model.StrankaZaposleni.Count <= 0)
            {
                model.StrankaZaposleni = new List<ClientEmployeeModel>();
                int employeeID = CommonMethods.ParseInt(ComboBoxZaposleniStranke.Value.ToString());
                if (employeeID > 0)
                {
                    ClientEmployeeModel clientEmployee = new ClientEmployeeModel();
                    clientEmployee.idOsebe = employeeID;
                    clientEmployee.idStranka = model.idStranka;
                    clientEmployee.ts = DateTime.Now;
                    clientEmployee.tsIDOsebe = PrincipalHelper.GetUserPrincipal().ID;
                    model.StrankaZaposleni.Add(clientEmployee);
                }
            }
            else
            {
                model.StrankaZaposleni[0].idOsebe = CommonMethods.ParseInt(ComboBoxZaposleniStranke.Value.ToString());
            }

            model.KodaStranke = txtSifraStranke.Text;
            model.NazivPrvi = txtNazivPrvi.Text;
            model.NazivDrugi = txtNazivDrugi.Text;
            model.Naslov = txtNaslov.Text;
            model.StevPoste = txtStevPoste.Text;
            model.NazivPoste = txtNazivPoste.Text;
            model.Email = txtEmail.Text;
            model.Telefon = txtTelefon.Text;
            model.FAX = txtFax.Text;
            model.InternetniNalov = txtInternetniNalov.Text;
            model.KontaktnaOseba = txtKontaktnaOseba.Text;
            model.RokPlacila = txtRokPlacila.Text;
            model.RangStranke = txtRangStranke.Text;
            model.Aktivnost = CommonMethods.ParseInt(cmbAktivnost.SelectedItem.Value);

            ClientFullModel returnModel = CheckModelValidation(GetDatabaseConnectionInstance().SaveClientChanges(model));

            if (returnModel != null)
            {
                //this we need if we want to add new client and then go and add new Plan with no redirection to Clients page
                GetClientDataProviderInstance().SetClientFullModel(returnModel);


                //TODO: ADD new item to session and if user has added new client and create data bind.
                return true;
            }
            else
                return false;
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            bool isValid = false;
            bool isDeleteing = false;

            switch (action)
            {
                case (int)Enums.UserAction.Add:
                    isValid = AddOrEditEntityObject(true);
                    break;
                case (int)Enums.UserAction.Edit:
                    isValid = AddOrEditEntityObject();
                    break;
                case (int)Enums.UserAction.Delete:
                    isValid = DeleteObject();
                    isDeleteing = true;
                    break;
            }

            if (isValid)
            {
                ClearSessionsAndRedirect(isDeleteing);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearSessionsAndRedirect();
        }

        private bool DeleteObject()
        {
            return CheckModelValidation(GetDatabaseConnectionInstance().DeleteClient(clientID));
        }

        protected void ComboBoxZaposleniStranke_DataBinding(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();

            List<EmployeeSimpleModel> employees = GetEmployeeDataProviderInstance().GetEmployeesFromSession();

            if (employees == null)//if the session is empty then we go to the server to fetch employees
                employees = CheckModelValidation<List<EmployeeSimpleModel>>(GetDatabaseConnectionInstance().GetAllCompanyEmployees());

            if (employees != null)
            {
                GetEmployeeDataProviderInstance().AddEmployeesToSession(employees);
                string listEmployees = JsonConvert.SerializeObject(employees);
                dt = JsonConvert.DeserializeObject<DataTable>(listEmployees);
                dt.Columns.Add("CelotnoIme", typeof(string), "Ime + ' ' + Priimek");
                DataRow row = dt.NewRow();
                row["idOsebe"] = -1;
                row["Ime"] = "Izberi...";
                row["Priimek"] = "";
                dt.Rows.InsertAt(row, 0);
            }

            (sender as ASPxComboBox).DataSource = dt;
        }

        #region Plan
        protected void PlanCallback_Callback(object sender, DevExpress.Web.CallbackEventArgsBase e)
        {
            /*if (e.Parameter == "RefreshGrid")
            {
                InitializeEditDeleteButtons();
                ASPxGridViewPlan.DataBind();
            }
            else
            {
                object valueID = null;
                if (ASPxGridViewPlan.VisibleRowCount > 0)
                    valueID = ASPxGridViewPlan.GetRowValues(ASPxGridViewPlan.FocusedRowIndex, "idPlan");

                bool isValid = SetSessionsAndOpenPopUp(e.Parameter, Enums.ClientSession.PlanPopUpID, valueID);
                if (isValid)
                    ASPxPopupControl_Plan.ShowOnPageLoad = true;
            }*/
        }

        protected void ASPxGridViewPlan_DataBinding(object sender, EventArgs e)
        {/*
            if (CheckClientExistInDB())
            {
                DataTable dt = new DataTable();
                CheckForClientName(model.Plan);
                string plan = JsonConvert.SerializeObject(model.Plan);
                dt = JsonConvert.DeserializeObject<DataTable>(plan);

                (sender as ASPxGridView).DataSource = dt;
                ASPxGridViewPlan.FocusedRowIndex = 0;
            }*/
        }
        #endregion

        #region ContactPerson
        protected void ContactPersonCallback_Callback(object sender, CallbackEventArgsBase e)
        {
            if (e.Parameter == "RefreshGrid")
            {
                InitializeEditDeleteButtons();
                ASPxGridViewContactPerson.DataBind();
            }
            else
            {
                object valueID = null;
                if (ASPxGridViewContactPerson.VisibleRowCount > 0)
                    valueID = ASPxGridViewContactPerson.GetRowValues(ASPxGridViewContactPerson.FocusedRowIndex, "idKontaktneOsebe");

                bool isValid = SetSessionsAndOpenPopUp(e.Parameter, Enums.ClientSession.ContactPersonPopUp, valueID);
                if (isValid)
                    ASPxPopupControlContactPerson.ShowOnPageLoad = true;
            }
        }

        protected void ASPxGridViewContactPerson_DataBinding(object sender, EventArgs e)
        {
            if (CheckClientExistInDB())
            {
                DataTable dt = new DataTable();
                CheckForClientName(model.KontaktneOsebe);
                string contactPersons = JsonConvert.SerializeObject(model.KontaktneOsebe);
                dt = JsonConvert.DeserializeObject<DataTable>(contactPersons);

                (sender as ASPxGridView).DataSource = dt;
                ASPxGridViewContactPerson.Settings.GridLines = GridLines.Both;
                ASPxGridViewContactPerson.FocusedRowIndex = 0;
                ASPxGridViewContactPerson.ScrollToVisibleIndexOnClient = 0;
            }
        }
        #endregion

        #region OtherHelperMethods Local
        private bool CheckClientExistInDB()
        {
            model = GetClientDataProviderInstance().GetFullModelFromClientModel();

            if (model == null && clientID == 0)
            {
                if (String.IsNullOrEmpty(txtSifraStranke.Text))
                    txtSifraStranke.Text = WebConfigurationManager.AppSettings["DefaultClientCode"].ToString();

                bool isValid = AddOrEditEntityObject(true);
                if (isValid)
                {
                    int idClient = GetClientDataProviderInstance().GetFullModelFromClientModel().idStranka;
                    var nameValues = HttpUtility.ParseQueryString(Request.QueryString.ToString());
                    nameValues.Set(Enums.QueryStringName.recordId.ToString(), idClient.ToString());

                    AddValueToSession(Enums.CommonSession.ShowWarning, true);
                    ASPxWebControl.RedirectOnCallback(GenerateURI("ClientsForm.aspx", (int)Enums.UserAction.Edit, idClient.ToString()));
                }
                return false;
            }

            return true;
        }


        private bool SetSessionsAndOpenPopUp(string eventParameter, Enums.ClientSession sessionToWrite, object entityID, int idCategorie = 0)
        {
            int callbackResult = 0;
            int.TryParse(eventParameter, out callbackResult);
            if (callbackResult > 0 && callbackResult <= 3)
            {
                switch (callbackResult)
                {
                    case (int)Enums.UserAction.Add:
                        AddValueToSession(Enums.CommonSession.UserActionPopUp, callbackResult);
                        AddValueToSession(sessionToWrite, 0);
                        AddValueToSession(Enums.ClientSession.ClientId, clientID);
                        break;

                    default://For editing and deleting is the same code.
                        AddValueToSession(Enums.CommonSession.UserActionPopUp.ToString(), callbackResult);
                        AddValueToSession(sessionToWrite, entityID);
                        AddValueToSession(Enums.ClientSession.ClientId, clientID);
                        if (idCategorie > 0)
                            AddValueToSession(Enums.ClientSession.CategorieID, idCategorie);
                        break;

                }
                return true;
            }

            return false;
        }

        private void ClearSessionsAndRedirect(bool isIDDeleted = false, string pageToRedirect = "Clients.aspx", bool isCallback = false)
        {
            QueryStrings item = new QueryStrings() { Attribute = Enums.QueryStringName.recordId.ToString(), Value = clientID.ToString() };
            string redirectString = "";

            if (isIDDeleted)
                redirectString = pageToRedirect;
            else
                redirectString = GenerateURI(pageToRedirect, item);

            List<Enums.ClientSession> list = Enum.GetValues(typeof(Enums.ClientSession)).Cast<Enums.ClientSession>().ToList();

            ClearSessions();
            ClearAllSessions(list, redirectString, isCallback);
        }

        private void ClearSessions()
        {
            RemoveSession(Enums.EmployeeSession.EmployeesList);
            RemoveSession(Enums.ChartSession.GraphCollection);

            List<Enums.ClientSession> list = Enum.GetValues(typeof(Enums.ClientSession)).Cast<Enums.ClientSession>().ToList();
            ClearAllSessions(list);
        }

        private void CheckForClientName(List<PlanModel> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (String.IsNullOrEmpty(item.Stranka))
                        item.Stranka = txtNazivPrvi.Text;
                }
            }
        }
        private void CheckForClientName(List<ContactPersonModel> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (String.IsNullOrEmpty(item.Stranka))
                        item.Stranka = txtNazivPrvi.Text;
                }
            }
        }
        private void CheckForClientName(List<DevicesModel> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (String.IsNullOrEmpty(item.Stranka))
                        item.Stranka = txtNazivPrvi.Text;
                }
            }
        }

        private void CheckForClientName(List<NotesModel> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (String.IsNullOrEmpty(item.Stranka))
                        item.Stranka = txtNazivPrvi.Text;
                }
            }
        }

        private void CheckForClientName(List<ClientCategorieModel> list)
        {
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (String.IsNullOrEmpty(item.Stranka))
                        item.Stranka = txtNazivPrvi.Text;
                }
            }
        }

        private List<ChartRenderSimple> CheckForMissingMoths(List<ChartRenderSimple> list, int period, int type, int categorieID, decimal valuePrice = 0)
        {
            if (list.Count > 0)
            {
                DateTime currentDate = list[0].Datum;

                for (int i = 0; i < list.Count; i++)
                {
                    bool exist = list.Any(dat => dat.Datum.Month == currentDate.Month && dat.Datum.Year == currentDate.Year);
                    if (!exist)
                    {
                        ChartRenderSimple tmpModel = new ChartRenderSimple();
                        tmpModel.Datum = currentDate.Date;
                        tmpModel.EnotaMere = "";
                        tmpModel.IzpisGrafaID = 0;
                        tmpModel.KategorijaID = categorieID;
                        tmpModel.Obdobje = period;
                        tmpModel.Opis = currentDate.Date.ToString("MMMM yyyy");
                        tmpModel.StrankaID = clientID;
                        tmpModel.Tip = type;
                        tmpModel.Vrednost = valuePrice;
                        list.Insert(list.IndexOf(list[i]), tmpModel);
                    }
                    else
                        currentDate = currentDate.AddMonths(1);
                }
            }
            return list;
        }

        private HtmlTableRow AddChartsToCell(UserControlGraph ucg, HtmlTableRow row, int graphsInRow)
        {
            HtmlTableRow newTRow = new HtmlTableRow();
            HtmlTableCell tCell = null;

            if (row.Cells.Count < graphsInRow)
            {
                if (row.Cells.Count == 0)//we add bottom border only the first time
                    row.Style.Add("border-bottom", "solid 1px black");

                tCell = new HtmlTableCell();
                tCell.Controls.Add(ucg);
                row.Cells.Add(tCell);
            }
            else
            {
                tCell = new HtmlTableCell();
                tCell.Controls.Add(ucg);
                newTRow.Cells.Add(tCell);
                newTRow.Style.Add("border-bottom", "solid 1px black");

                return newTRow;
            }

            return row;
        }
        #endregion

        #region Events
        protected void EventsCallback_Callback(object sender, CallbackEventArgsBase e)
        {
            if (e.Parameter == "2" || e.Parameter == "1")
            {
                object valueID = null;
                if (ASPxGridViewEvents.VisibleRowCount > 0)
                    valueID = ASPxGridViewEvents.GetRowValues(ASPxGridViewEvents.FocusedRowIndex, "idDogodek");

                if (e.Parameter == "1") valueID = 0;

                if (valueID != null)
                {
                    int employeeID = 0;
                    if (model != null)
                        employeeID = model.StrankaZaposleni.Count <= 0 ? 0 : model.StrankaZaposleni[0].idOsebe;

                    if (employeeID == 0)
                    {
                        EventsCallback.JSProperties["cpCallbackError"] = "Komercialist ni dodeljen stranki! \r\n V osnovnih podatkih nastavi Skrbnika";
                        return;
                    }
                    List<QueryStrings> queryList = new List<QueryStrings>();

                    QueryStrings item = new QueryStrings() { Attribute = Enums.QueryStringName.action.ToString(), Value = (e.Parameter == "1" ? ((int)Enums.UserAction.Add).ToString() : ((int)Enums.UserAction.Edit).ToString())};
                    queryList.Add(item);
                    item = new QueryStrings() { Attribute = Enums.QueryStringName.recordId.ToString(), Value = valueID.ToString() };
                    queryList.Add(item);
                    item = new QueryStrings() { Attribute = Enums.QueryStringName.eventClientId.ToString(), Value = clientID.ToString() };
                    queryList.Add(item);
                    item = new QueryStrings() { Attribute = Enums.QueryStringName.eventCategorieId.ToString(), Value = (-1).ToString() };
                    queryList.Add(item);
                    item = new QueryStrings() { Attribute = Enums.QueryStringName.eventEmployeeId.ToString(), Value = employeeID.ToString() };
                    queryList.Add(item);

                    AddValueToSession(Enums.CommonSession.activeTab, "Events");

                    ClearSessionsAndRedirect(true, GenerateURI("../Events/EventsForm.aspx", queryList), true);
                }
                //}
                //else if (e.Parameter == "1")
                //{
                //    List<QueryStrings> queryList = new List<QueryStrings>();

                //    int employeeID = 0;
                //    if (model != null)
                //        employeeID = model.StrankaZaposleni[0].idOsebe;

                //    QueryStrings item = new QueryStrings() { Attribute = Enums.QueryStringName.action.ToString(), Value = "1" };
                //    queryList.Add(item);
                //    item = new QueryStrings() { Attribute = Enums.QueryStringName.recordId.ToString(), Value = "0" };
                //    queryList.Add(item);
                //    item = new QueryStrings() { Attribute = Enums.QueryStringName.eventClientId.ToString(), Value = clientID.ToString() };
                //    queryList.Add(item);
                //    item = new QueryStrings() { Attribute = Enums.QueryStringName.eventCategorieId.ToString(), Value = (-1).ToString() };
                //    queryList.Add(item);
                //    item = new QueryStrings() { Attribute = Enums.QueryStringName.eventEmployeeId.ToString(), Value = employeeID.ToString() };
                //    queryList.Add(item);

                //    AddValueToSession(Enums.CommonSession.activeTab, "Basic");



            }
        }


        protected void ASPxGridViewEvents_DataBinding(object sender, EventArgs e)
        {
            if (CheckClientExistInDB())
            {
                DataTable dt = new DataTable();
                string events = JsonConvert.SerializeObject(model.Dogodek);
                dt = JsonConvert.DeserializeObject<DataTable>(events);

                (sender as ASPxGridView).DataSource = dt;
                ASPxGridViewEvents.FocusedRowIndex = 0;
                ASPxGridViewEvents.Settings.GridLines = GridLines.Both;
            }
        }
        #endregion

        #region Devices
        protected void DeviceCallback_Callback(object sender, CallbackEventArgsBase e)
        {
            if (e.Parameter == "RefreshGrid")
            {
                InitializeEditDeleteButtons();
                ASPxGridViewDevice.DataBind();
            }
            else
            {
                object valueID = null;
                if (ASPxGridViewDevice.VisibleRowCount > 0)
                    valueID = ASPxGridViewDevice.GetRowValues(ASPxGridViewDevice.FocusedRowIndex, "idNaprava");

                bool isValid = SetSessionsAndOpenPopUp(e.Parameter, Enums.ClientSession.DevicePopUpID, valueID);
                if (isValid)
                    ASPxPopupControlDevice.ShowOnPageLoad = true;
            }
        }

        protected void ASPxGridViewDevice_DataBinding(object sender, EventArgs e)
        {
            if (CheckClientExistInDB())
            {
                DataTable dt = new DataTable();
                CheckForClientName(model.Naprave);
                string devices = JsonConvert.SerializeObject(model.Naprave);
                dt = JsonConvert.DeserializeObject<DataTable>(devices);

                (sender as ASPxGridView).DataSource = dt;
                ASPxGridViewDevice.FocusedRowIndex = 0;
                ASPxGridViewDevice.Settings.GridLines = GridLines.Both;
            }
        }
        #endregion

        #region Notes
        protected void NotesCallback_Callback(object sender, CallbackEventArgsBase e)
        {
            if (e.Parameter == "RefreshGrid")
            {
                InitializeEditDeleteButtons();
                ASPxGridViewNotes.DataBind();
            }
            else
            {
                object valueID = null;
                if (ASPxGridViewNotes.VisibleRowCount > 0)
                    valueID = ASPxGridViewNotes.GetRowValues(ASPxGridViewNotes.FocusedRowIndex, "idOpombaStranka");

                bool isValid = SetSessionsAndOpenPopUp(e.Parameter, Enums.ClientSession.NotesPopUpID, valueID);
                if (isValid)
                    ASPxPopupControlNotes.ShowOnPageLoad = true;
            }
        }

        protected void ASPxGridViewNotes_DataBinding(object sender, EventArgs e)
        {
            if (CheckClientExistInDB())
            {
                model = CheckModelValidation<ClientFullModel>(GetDatabaseConnectionInstance().GetClient(clientID));

                DataTable dt = new DataTable();
                CheckForClientName(model.Opombe);
                string Notes = JsonConvert.SerializeObject(model.Opombe);
                dt = JsonConvert.DeserializeObject<DataTable>(Notes);

                (sender as ASPxGridView).DataSource = dt;
                ASPxGridViewNotes.FocusedRowIndex = 0;
                ASPxGridViewNotes.Settings.GridLines = GridLines.Both;
            }
        }
        #endregion

        #region Categorie
        protected void CategorieCallback_Callback(object sender, CallbackEventArgsBase e)
        {
            if (e.Parameter == "RefreshGrid")
            {
                InitializeEditDeleteButtons();
                ASPxGridViewCategorie.DataBind();
            }
            else
            {
                object valueID = null;
                object idCategorie = 0;
                if (ASPxGridViewCategorie.VisibleRowCount > 0)
                {
                    valueID = ASPxGridViewCategorie.GetRowValues(ASPxGridViewCategorie.FocusedRowIndex, "idStrankaKategorija");
                    idCategorie = ASPxGridViewCategorie.GetRowValues(ASPxGridViewCategorie.FocusedRowIndex, "idKategorija");
                }

                bool isValid = SetSessionsAndOpenPopUp(e.Parameter, Enums.ClientSession.ClientCategoriePopUpID, valueID, CommonMethods.ParseInt(idCategorie));
                if (isValid)
                    ASPxPopupControlCategorie.ShowOnPageLoad = true;
            }
        }


        protected void ASPxGridViewCategorie_DataBinding(object sender, EventArgs e)
        {
            if (CheckClientExistInDB())
            {
                DataTable dt = new DataTable();
                CheckForClientName(model.StrankaKategorija);
                //string clientCategorie = JsonConvert.SerializeObject(model.StrankaKategorija);
                //dt = JsonConvert.DeserializeObject<DataTable>(clientCategorie);

                dt.Columns.Add("idStrankaKategorija");
                dt.Columns.Add("idKategorija");
                dt.Columns.Add("Stranka");
                dt.Columns.Add("Koda");
                dt.Columns.Add("Naziv");
                dt.Columns.Add("ts");
                dt.Columns.Add("tsIDOseba");
                dt.PrimaryKey = new DataColumn[] { dt.Columns["idStrankaKategorija"] };

                foreach (var item in model.StrankaKategorija)
                {
                    DataRow dr = dt.NewRow();
                    dr["idStrankaKategorija"] = item.idStrankaKategorija;
                    dr["idKategorija"] = item.idKategorija;
                    dr["Stranka"] = item.Stranka;
                    dr["Koda"] = item.Kategorija.Koda;
                    dr["Naziv"] = item.Kategorija.Naziv;
                    dr["ts"] = item.ts;
                    dr["tsIDOseba"] = item.tsIDOseba;
                    dt.Rows.Add(dr);
                }

                (sender as ASPxGridView).DataSource = dt;
                ASPxGridViewCategorie.FocusedRowIndex = 0;
                ASPxGridViewCategorie.Settings.GridLines = GridLines.Both;
            }
        }
        #endregion

        #region Charts
        private void ucf2_btnPostClk(object sender, EventArgs e)
        {
            UserControlGraph ucf2 = (UserControlGraph)sender;

            int period = CommonMethods.ParseInt(ucf2.Period.SelectedItem.Value);
            int type = CommonMethods.ParseInt(ucf2.Type.SelectedItem.Value);
            if (period != (int)Enums.ChartRenderPeriod.TEDENSKO)
            {
                ChartRenderModel chart = CheckModelValidation(GetDatabaseConnectionInstance().GetChartDataFromSQLFunction(clientID, ucf2.CategorieID, period, type));

                if (period == (int)Enums.ChartRenderPeriod.MESECNO)
                    chart.chartRenderData = CheckForMissingMoths(chart.chartRenderData, period, type, ucf2.CategorieID, 0);


                GetClientDataProviderInstance().GetGraphBindingList().Find(gb => gb.CategorieID == ucf2.CategorieID).chartData = chart;
                GetClientDataProviderInstance().GetGraphBindingList().Find(gb => gb.CategorieID == ucf2.CategorieID).YAxisTitle = chart.chartRenderData.Count > 0 ? chart.chartRenderData[0].EnotaMere : "";
                GetClientDataProviderInstance().GetGraphBindingList().Find(gb => gb.CategorieID == ucf2.CategorieID).obdobje = period;
                GetClientDataProviderInstance().GetGraphBindingList().Find(gb => gb.CategorieID == ucf2.CategorieID).tip = type;

                ucf2.CreateGraph(chart);
            }
            else
            {
                int previousPeriod = GetClientDataProviderInstance().GetGraphBindingList().Find(gb => gb.CategorieID == ucf2.CategorieID).obdobje;
                ucf2.Period.SelectedIndex = ucf2.Period.Items.IndexOf(ucf2.Period.Items.FindByValue(previousPeriod.ToString()));
            }
        }
        private void ucf2_btnDeleteGraphClick(object sender, EventArgs e)
        { }

        protected void ChartsCallback_Callback(object sender, CallbackEventArgsBase e)
        {
            List<GraphBinding> list = GetClientDataProviderInstance().GetGraphBindingList();

            if (!String.IsNullOrEmpty(e.Parameter))
            {
                if (e.Parameter.Equals("ThreeInRow"))
                    chartsInRow = 3;
                else if (e.Parameter.Equals("TwoInRow"))
                    chartsInRow = 2;

                else if (e.Parameter.Equals("OneInRow"))
                    chartsInRow = 1;

                GetClientDataProviderInstance().SetChartsCoutInRow(chartsInRow);
                ASPxWebControl.RedirectOnCallback(GenerateURI("ClientsForm.aspx", (int)Enums.UserAction.Edit, clientID.ToString()));
                /*ChartsCallback.Controls.Clear();
                AddControlsToPanel(list);*/
            }

            if (model == null)
                ShowClientPopUp("First save the client and add some categories to view the charts.");

            if (list == null)
                CreateCharts();
            else if (model.StrankaKategorija.Count(cat => cat.HasChartDataForCategorie) != list.Count)
            {
                GetClientDataProviderInstance().SetGraphBindingList(null);
                ASPxWebControl.RedirectOnCallback(GenerateURI("ClientsForm.aspx", (int)Enums.UserAction.Edit, clientID.ToString()));
                //We use this if the user is changing tabs(if the user has deleted one of the categories wee need to refresh it)
            }

        }
        private void CreateCharts()
        {
            List<GraphBinding> bindingCollection = new List<GraphBinding>();
            HtmlTable table = new HtmlTable();
            table.Style.Add("width", "100%");

            bindingCollection.Select(item => item.control).ToList();//get all controls and only controls from list

            if (model.StrankaKategorija == null) return;

            GetClientDataProviderInstance().SetMainContentWidthForCharts(CommonMethods.ParseDouble(hiddenField["browserWidth"]));
            GetClientDataProviderInstance().SetChartsCoutInRow(model.StrankaKategorija.Count > 3 ? 3 : model.StrankaKategorija.Count);

            HtmlTableRow tRow = new HtmlTableRow();

            CommonMethods.LogThis("Metoda: CreateCharts => pred foreach \r\n");
            foreach (var item in model.StrankaKategorija)
            {
                CommonMethods.LogThis("Metoda: CreateCharts => pred nalaganje user contorls \r\n");
                UserControlGraph ucf2 = (UserControlGraph)LoadControl("~/UserControls/UserControlGraph.ascx");
                CommonMethods.LogThis("Metoda: CreateCharts => pred klicem web service sql funkcije \r\n");
                ChartRenderModel chart = CheckModelValidation(GetDatabaseConnectionInstance().GetChartDataFromSQLFunction(clientID, item.Kategorija.idKategorija, (int)Enums.ChartRenderPeriod.LETNO, (int)Enums.ChartRenderType.KOLICINA));
                CommonMethods.LogThis("Metoda: CreateCharts => po klicu web service sql funkcije \r\n");
                if (chart == null)
                    CommonMethods.LogThis("Metoda: CreateCharts => chart instanca je null! \r\n");
                else
                    CommonMethods.LogThis("Metoda: CreateCharts => Count ChartRenderData" + chart.chartRenderData.Count.ToString() + "\r\n");

                if (chart.chartRenderData.Count > 0)
                {
                    item.HasChartDataForCategorie = true;
                    //ucf2.ID = model.KodaStranke + "_UserControlGraph_" + (bindingCollection.Count + 1).ToString();
                    ucf2.btnPostClk += ucf2_btnPostClk;
                    ucf2.btnDeleteGraphClick += ucf2_btnDeleteGraphClick;
                    ucf2.btnAddEventClick += ucf2_btnAddEventClick;


                    tRow = AddChartsToCell(ucf2, tRow, 3);
                    table.Rows.Add(tRow);

                    ChartsCallback.Controls.Add(table);

                    GraphBinding instance = new GraphBinding();

                    //chart.chartRenderData = CheckForMissingMoths(chart.chartRenderData, (int)Enums.ChartRenderPeriod.LETNO, (int)Enums.ChartRenderType.KOLICINA, item.Kategorija.idKategorija, 0);

                    ucf2.HeaderName.HeaderText = item.Kategorija.Koda;
                    ucf2.HeaderLink.NavigateUrl = "../Pages/CodeList/Clients/ClientsChartsDetail.aspx?categorieId=" + item.idKategorija + "&recordId=" + clientID.ToString();
                    ucf2.Period.SelectedIndex = ucf2.Period.Items.FindByValue(((int)Enums.ChartRenderPeriod.LETNO).ToString()).Index;
                    ucf2.Period.Visible = false;
                    ucf2.Type.SelectedIndex = ucf2.Type.Items.FindByValue(((int)Enums.ChartRenderType.KOLICINA).ToString()).Index;
                    ucf2.RenderChart.Text = "Izriši " + item.Kategorija.Koda;
                    ucf2.CategorieID = item.Kategorija.idKategorija;
                    ucf2.YAxisTitle = chart.chartRenderData.Count > 0 ? chart.chartRenderData[0].EnotaMere : "";
                    ucf2.CreateGraph(chart);

                    instance.obdobje = (int)Enums.ChartRenderPeriod.LETNO;
                    instance.tip = (int)Enums.ChartRenderType.KOLICINA;
                    instance.YAxisTitle = ucf2.YAxisTitle;
                    instance.chartData = chart;
                    instance.control = ucf2;
                    instance.HeaderText = item.Kategorija.Koda;
                    instance.CategorieID = item.Kategorija.idKategorija;
                    bindingCollection.Add(instance);
                }
            }
            GetClientDataProviderInstance().SetClientFullModel(model);//we set new model because it might changed in the procees of filling data for charts (if there is chart data we change status to true on ClientCategorieModel item)
            GetClientDataProviderInstance().SetGraphBindingList(bindingCollection);
        }

        private void addEvent(object sender)
        {
            UserControlGraph ucf2 = (UserControlGraph)sender;
            if (PrincipalHelper.GetUserPrincipal().HasSupervisor && (model.StrankaZaposleni != null && model.StrankaZaposleni.Count > 0))
            {
                List<QueryStrings> queryList = new List<QueryStrings>();

                int employeeID = 0;
                if (model != null)
                    employeeID = model.StrankaZaposleni[0].idOsebe;

                QueryStrings item = new QueryStrings() { Attribute = Enums.QueryStringName.action.ToString(), Value = "1" };
                queryList.Add(item);
                item = new QueryStrings() { Attribute = Enums.QueryStringName.recordId.ToString(), Value = "0" };
                queryList.Add(item);
                item = new QueryStrings() { Attribute = Enums.QueryStringName.eventClientId.ToString(), Value = clientID.ToString() };
                queryList.Add(item);
                item = new QueryStrings() { Attribute = Enums.QueryStringName.eventCategorieId.ToString(), Value = ucf2 != null ? ucf2.CategorieID.ToString() : (-1).ToString() };
                queryList.Add(item);
                item = new QueryStrings() { Attribute = Enums.QueryStringName.eventEmployeeId.ToString(), Value = employeeID.ToString() };
                queryList.Add(item);

                AddValueToSession(Enums.CommonSession.activeTab, (ucf2 != null ? "Charts" : "Basic"));
                ClearSessionsAndRedirect(true, GenerateURI("../Events/EventsForm.aspx", queryList));
            }
            else
            {

                ClientPageControl.ActiveTabIndex = ClientPageControl.TabPages.FindByName("Basic").Index;
                ErrorLabel.Text = "Skrbnik in zaposlen za stranko ni izbran!";
            }
        }

        private void ucf2_btnAddEventClick(object sender, EventArgs e)
        {
            addEvent(sender);
        }

        #endregion

        #region Initilizations
        private void Initialize()
        {
            ComboBoxZaposleniStranke.DataBind();
        }

        private void InitializeEditDeleteButtons()
        {
            //Check to enable Edit and Delete button for Tab PLAN
            if (model == null || (model.Plan == null || model.Plan.Count <= 0))
            {
                EnabledDeleteAndEditBtnPopUp(btnEditPlan, btnDeletePlan);
            }
            else if (!btnDeletePlan.Enabled && !btnEditPlan.Enabled)
            {
                EnabledDeleteAndEditBtnPopUp(btnEditPlan, btnDeletePlan, false);
            }

            //Check to enable Edit and Delete button for Tab CONTACT PERSON
            if (model == null || (model.KontaktneOsebe == null || model.KontaktneOsebe.Count <= 0))
            {
                EnabledDeleteAndEditBtnPopUp(btnEditContactPerson, btndeleteContactPerson);
            }
            else if (!btnEditContactPerson.Enabled && !btndeleteContactPerson.Enabled)
            {
                EnabledDeleteAndEditBtnPopUp(btnEditContactPerson, btndeleteContactPerson, false);
            }

            //Check to enable Edit and Delete button for Tab EVENTS
            if (model == null || (model.Dogodek == null || model.Dogodek.Count <= 0))
            {
                EnabledDeleteAndEditBtnPopUp(btnEditEvent, btnDeleteEvent);
            }
            else if (!btnEditEvent.Enabled && !btnDeleteEvent.Enabled)
            {
                EnabledDeleteAndEditBtnPopUp(btnEditEvent, btnDeleteEvent, false);
            }

            //Check to enable Edit and Delete button for Tab DEVICES
            if (model == null || (model.Naprave == null || model.Naprave.Count <= 0))
            {
                EnabledDeleteAndEditBtnPopUp(btnEditDevice, btnDeleteDevice);
            }
            else if (!btnEditDevice.Enabled && !btnDeleteDevice.Enabled)
            {
                EnabledDeleteAndEditBtnPopUp(btnEditDevice, btnDeleteDevice, false);
            }

            //Check to enable Edit and Delete button for Tab OPOMBE
            if (model == null || (model.Opombe == null || model.Opombe.Count <= 0))
            {
                EnabledDeleteAndEditBtnPopUp(btnEditNotes, btnDeleteNotes);
            }
            else if (!btnEditNotes.Enabled && !btnDeleteNotes.Enabled)
            {
                EnabledDeleteAndEditBtnPopUp(btnEditNotes, btnDeleteNotes, false);
            }

            //Check to enable Edit and Delete button for Tab CATEGORIE
            if (model == null || (model.StrankaKategorija == null || model.StrankaKategorija.Count <= 0))
            {
                EnabledDeleteAndEditBtnPopUp(btnEditCategorie, btnDeleteCategorie);
            }
            else if (!btnEditCategorie.Enabled && !btnDeleteCategorie.Enabled)
            {
                EnabledDeleteAndEditBtnPopUp(btnEditCategorie, btnDeleteCategorie, false);
            }
        }

        private void AddControlsToPanel(List<GraphBinding> collection)
        {
            HtmlTableRow tRow = new HtmlTableRow();
            HtmlTable table = new HtmlTable();
            table.Style.Add("width", "100%");

            foreach (GraphBinding item in collection)
            {
                UserControlGraph ucf2 = (UserControlGraph)LoadControl("~/UserControls/UserControlGraph.ascx");
                //ucf2.ID = model.KodaStranke + "_UserControlGraph_" + (collection.Count + 1).ToString();
                ucf2.btnPostClk += ucf2_btnPostClk;
                ucf2.btnDeleteGraphClick += ucf2_btnDeleteGraphClick;
                ucf2.btnAddEventClick += ucf2_btnAddEventClick;

                tRow = AddChartsToCell(ucf2, tRow, GetClientDataProviderInstance().GetChartsCoutInRow());
                table.Rows.Add(tRow);

                ChartsCallback.Controls.Add(table);
                ucf2.Period.SelectedIndex = ucf2.Period.Items.FindByValue((item.obdobje >= 0 ? item.obdobje : 0).ToString()).Index;
                ucf2.Period.Visible = false;
                ucf2.Type.SelectedIndex = ucf2.Type.Items.FindByValue((item.tip >= 0 ? item.tip : 0).ToString()).Index;
                ucf2.HeaderName.HeaderText = item.HeaderText;
                ucf2.HeaderLink.NavigateUrl = "../Pages/CodeList/Clients/ClientsChartsDetail.aspx?categorieId=" + item.CategorieID + "&recordId=" + clientID.ToString();
                ucf2.RenderChart.Text = "Izriši " + item.HeaderText;
                ucf2.CategorieID = item.CategorieID;
                ucf2.YAxisTitle = item.YAxisTitle;
                ucf2.CreateGraph(item.chartData);
            }
        }
        #endregion

        protected void AutoAddEventCallback_Callback(object source, DevExpress.Web.CallbackEventArgs e)
        {
            if (CheckClientExistInDB() && PrincipalHelper.GetUserPrincipal().HasSupervisor)
            {
                //TODO: ADD event
                EventFullModel eventmodel = new EventFullModel();
                eventmodel.DatumOtvoritve = DateTime.Now;
                eventmodel.idDogodek = 0;
                eventmodel.idStranka = clientID;
                eventmodel.Izvajalec = CommonMethods.ParseInt(ComboBoxZaposleniStranke.Value);
                eventmodel.Opis = "Redni obisk na stranki...";
                eventmodel.Rok = DateTime.Now;
                eventmodel.Tip = "Samodejni dogodek";

                eventmodel = CheckModelValidation(GetDatabaseConnectionInstance().SaveEventChanges(eventmodel));
                e.Result = "Uspešno si dodal nov dogodek na stranko.";

                if (eventmodel == null)
                    e.Result = "Neuspešno dodajanje dogodka. Kontaktiraj administratorja.";
            }
            else
                e.Result = "Te stranke še ni v bazi oz. skrbnika za osebo ni izbranega.";

            if (model.StrankaZaposleni == null || model.StrankaZaposleni.Count <= 0)
                e.Result += " Potrebno je shraniti tudi zaposlenega na stranko!";
        }

        protected void addEventBtn_Click(object sender, EventArgs e)
        {
            if (PrincipalHelper.GetUserPrincipal().HasSupervisor)
                ucf2_btnAddEventClick(null, e);
            else
                ErrorLabel.Text = "Skrbnik za osebo ni izbran!";
        }
    }
}