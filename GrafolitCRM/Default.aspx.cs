﻿using AnalizaProdaje.Common;
using AnalizaProdaje.Domain.Concrete;
using AnalizaProdaje.Domain.Helpers;
using AnalizaProdaje.UserControls.Widgets;
using DatabaseWebService.Models;
using DatabaseWebService.Models.Client;
using DevExpress.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AnalizaProdaje
{
    public partial class Default : ServerMasterPage
    {

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (Request.IsAuthenticated)
                MasterPageFile = "~/MasterPage.Master";
            else
            {
                Master.LoginButtonEvent += new EventHandler(ASPxButtonLogin_Click);
                widgetSection.Visible = false;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                ASPxButtonLogin.Visible = false;
                ASPxRoundPanel1.HeaderText = "Dobrodošli v aplikaciji ANALIZA PRODAJE";
                RemoveAllSesions();
            }
        }

        protected void ASPxButtonLogin_Click(object sender, EventArgs e)
        {
            /*((ASPxNavBar)Master.FindControl("ASPxNavBarMainMenu")).Enabled = false;*/
            if (Session["PreviousPage"] == null)
                Session["PreviousPage"] = Request.RawUrl;
            ASPxPopupControl1_Prijava.ShowOnPageLoad = true;
        }

        protected void eventsWidgetPanel_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                EventsWidget eventWidget = (EventsWidget)LoadControl("~/UserControls/Widgets/EventsWidget.ascx");

                if (PrincipalHelper.IsUserSalesman() || PrincipalHelper.IsUserUser())//if user is salesman we filter events by salesman
                    eventWidget.eventData = CheckModelValidation(GetDatabaseConnectionInstance().GetAllEvents(PrincipalHelper.GetUserPrincipal().ID));
                else//otherwise we get all events
                    eventWidget.eventData = CheckModelValidation(GetDatabaseConnectionInstance().GetAllEvents());

                if (eventWidget.eventData != null)
                {
                    var result = from myRows in eventWidget.eventData.AsEnumerable()
                                 where myRows.Field<DateTime>("Rok").CompareTo(DateTime.Now) >= 0
                                 select myRows;

                    if (result.Count() > 0)
                        eventWidget.eventData = result.CopyToDataTable();
                    else
                        eventWidget.eventData = new DataTable();

                    eventsWidgetPanel.Controls.Clear();
                    eventsWidgetPanel.Controls.Add(eventWidget);
                }

            }
        }
    }
}