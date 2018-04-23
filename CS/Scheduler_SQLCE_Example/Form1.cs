using DevExpress.XtraScheduler;
using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Windows.Forms;

namespace Scheduler_SQLCE_Example {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            this.schedulerStorage1.Appointments.ResourceSharing = true;

            //this.schedulerStorage1.Appointments.CommitIdToDataSource = false;
            this.schedulerStorage1.AppointmentsInserted += schedulerStorage1_AppointmentsInserted;
            appointmentsTableAdapter.Adapter.RowUpdated += new SqlCeRowUpdatedEventHandler(appointmentsTableAdapter_RowUpdated);
        }
        private void appointmentsTableAdapter_RowUpdated(object sender, SqlCeRowUpdatedEventArgs e) {
            // Gets ID for a new appointment from the SQL CE data source
            if (e.Status == UpdateStatus.Continue && e.StatementType == StatementType.Insert) {
                int id = 0;
                using (SqlCeCommand cmd = new SqlCeCommand("SELECT @@IDENTITY",
                    appointmentsTableAdapter.Connection)) {
                    id = Convert.ToInt32(cmd.ExecuteScalar());
                }
                e.Row["UniqueID"] = id;
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            // TODO: This line of code loads data into the 'dXDBDataSet2.Resources' table. You can move, or remove it, as needed.
            this.resourcesTableAdapter.Fill(this.dXDBDataSet2.Resources);
            // TODO: This line of code loads data into the 'dXDBDataSet2.Appointments' table. You can move, or remove it, as needed.
            this.appointmentsTableAdapter.Fill(this.dXDBDataSet2.Appointments);
        }

        private void schedulerStorage1_AppointmentChanging(object sender, DevExpress.XtraScheduler.PersistentObjectCancelEventArgs e) {
            string apt_id = string.Empty;
            if (((Appointment)e.Object).Id == null)
                apt_id = "null";
            else
                apt_id = ((Appointment)e.Object).Id.ToString();
            MessageBox.Show("AppointmentChanging event for " + apt_id);
        }

        private void OnApptChangedInsertedDeleted(object sender, PersistentObjectsEventArgs e) {
            UpdateSource();
        }

        void schedulerStorage1_AppointmentsInserted(object sender, PersistentObjectsEventArgs e) {
            UpdateSource();
            // Synchronize appointment IDs in the storage with the SQL CE data source
            AppointmentBaseCollection apts = e.Objects as AppointmentBaseCollection;
            foreach (Appointment apt in apts)
            {
                DataRowView dataRow = apt.GetSourceObject(this.schedulerStorage1) as DataRowView;
                this.schedulerStorage1.SetAppointmentId(apt, dataRow.Row["UniqueID"]);
            }
        }

        void UpdateSource() {
            appointmentsTableAdapter.Update(dXDBDataSet2);
            dXDBDataSet2.AcceptChanges();
        }

    }
}
