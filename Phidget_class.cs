using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phidget22;
using Phidget22.Events;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Draw
{
    public class Phidget_class
    {

        RFID openCh = null;
        private RFID rfid; //Declare an RFID object
        Boolean runonce = false;
        private Object openArgs;

        public Phidget_class(RFID ch)
        {

            openCh = ch;

        }

        public void load()
        {
            rfid.Attach += rfid_Attach;
            rfid.Detach += rfid_Detach;
            rfid.Error += rfid_Error;

            rfid.Tag += rfid_Tag;
            rfid.TagLost += rfid_TagLost;

            try
            {
                //Create your Phidget channels
                rfid = new RFID();

                rfid.Open(5000);

                rfid.Open();
            }
            catch (PhidgetException ex) { MessageBox.Show("Load Error"); }
        }

        private void rfid_TagLost(object sender, RFIDTagLostEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void rfid_Tag(object sender, RFIDTagEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void rfid_Error(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void rfid_Detach(object sender, DetachEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void rfid_Attach(object sender, AttachEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
