﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArdupilotMega.Controls;
using ArdupilotMega.Controls.BackstageView;
using ArdupilotMega.Utilities;
using log4net;

namespace ArdupilotMega.GCSViews.ConfigurationView
{
    public partial class ConfigFriendlyParams : UserControl, IActivate
    {
        #region Class Fields

        private static readonly ILog log =
          LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ParameterMetaDataRepository _parameterMetaDataRepository;
        private Dictionary<string, string> _params = new Dictionary<string, string>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the parameter mode.
        /// </summary>
        /// <value>
        /// The parameter mode.
        /// </value>
        public string ParameterMode { get; set; }

        #endregion

        #region Constructor

        public ConfigFriendlyParams()
        {
            InitializeComponent();
            tableLayoutPanel1.Height = this.Height;
            _parameterMetaDataRepository = new ParameterMetaDataRepository();

            MainV2.comPort.ParamListChanged += comPort_ParamListChanged;
            Resize += this_Resize;

            BUT_rerequestparams.Click += BUT_rerequestparams_Click;
            BUT_writePIDS.Click += BUT_writePIDS_Click;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the BUT_writePIDS control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void BUT_writePIDS_Click(object sender, EventArgs e)
        {
            bool errorThrown = false;
            _params.ForEach(x =>
            {
                var matchingControls = tableLayoutPanel1.Controls.Find(x.Key, true);
                if (matchingControls.Length > 0)
                {
                    var ctl = (IDynamicParameterControl)matchingControls[0];
                    try
                    {
                        MainV2.comPort.setParam(x.Key, float.Parse(ctl.Value));
                    }
                    catch
                    {
                        errorThrown = true;
                        CustomMessageBox.Show("Set " + x.Key + " Failed");
                    }
                }
            });
            if (!errorThrown)
            {
                CustomMessageBox.Show("Parameters successfully saved.");
            }
        }

        /// <summary>
        /// Handles the Click event of the BUT_rerequestparams control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void BUT_rerequestparams_Click(object sender, EventArgs e)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
                return;

            ((Control)sender).Enabled = false;

            try
            {
                MainV2.comPort.getParamList();
            }
            catch (Exception ex)
            {
                log.Error("Exception getting param list", ex);
                CustomMessageBox.Show("Error: getting param list");
            }


            ((Control)sender).Enabled = true;

            BindParamList();
        }

        /// <summary>
        /// Handles the Resize event of the this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void this_Resize(object sender, EventArgs e)
        {
            tableLayoutPanel1.Height = this.Height - 50;
        }

        /// <summary>
        /// Handles the Load event of the ConfigRawParamsV2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void Activate()
        {
            BindParamList();
        }

        /// <summary>
        /// Handles the ParamListChanged event of the comPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void comPort_ParamListChanged(object sender, EventArgs e)
        {
            SortParamList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sorts the param list.
        /// </summary>
        private void SortParamList()
        {
            // Clear list
            _params.Clear();

            // When the parameter list is changed, re sort the list for our View's purposes
            MainV2.comPort.MAV.param.Keys.ForEach(x =>
            {
                string displayName = _parameterMetaDataRepository.GetParameterMetaData(x.ToString(), ParameterMetaDataConstants.DisplayName);
                string parameterMode = _parameterMetaDataRepository.GetParameterMetaData(x.ToString(), ParameterMetaDataConstants.User);

                // If we have a friendly display name AND
                if (!String.IsNullOrEmpty(displayName) &&
                    // The user type is equal to the ParameterMode specified at class instantiation OR
                      ((!String.IsNullOrEmpty(parameterMode) && parameterMode == ParameterMode) ||
                    // The user type is empty and this is in Advanced mode
                      String.IsNullOrEmpty(parameterMode) && ParameterMode == ParameterMetaDataConstants.Advanced))
                {
                    _params.Add(x.ToString(), displayName);
                }
            });
            _params = _params.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Binds the param list.
        /// </summary>
        private void BindParamList()
        {
            tableLayoutPanel1.Controls.Clear();
            if (_params == null || _params.Count == 0) SortParamList();

            // get the params if nothing exists already
            if (_params != null && _params.Count == 0)
            {
                try
                {
                    Utilities.ParameterMetaDataParser.GetParameterInformation();

                    _parameterMetaDataRepository.Reload();

                    SortParamList();
                }
                catch (Exception exp) { log.Error(exp); } // just to cleanup any errors
            }

            this.SuspendLayout();

            _params.OrderBy(x => x.Key).ForEach(x =>
         {
             AddControl(x);
         });

            this.ResumeLayout();
        }

        void AddControl(KeyValuePair<string,string> x)
        {
            if (!String.IsNullOrEmpty(x.Key))
            {
                try
                {
                    bool controlAdded = false;

                    string value = ((float)MainV2.comPort.MAV.param[x.Key]).ToString("0.###", CultureInfo.InvariantCulture);
                    string description = _parameterMetaDataRepository.GetParameterMetaData(x.Key, ParameterMetaDataConstants.Description);
                    string displayName = x.Value + " (" + x.Key + ")";
                    string units = _parameterMetaDataRepository.GetParameterMetaData(x.Key, ParameterMetaDataConstants.Units);

                    // If this is a range
                    string rangeRaw = _parameterMetaDataRepository.GetParameterMetaData(x.Key, ParameterMetaDataConstants.Range);
                    string incrementRaw = _parameterMetaDataRepository.GetParameterMetaData(x.Key, ParameterMetaDataConstants.Increment);
                    
                    if (!String.IsNullOrEmpty(rangeRaw) && !String.IsNullOrEmpty(incrementRaw))
                    {
                        float increment, intValue;
                        float.TryParse(incrementRaw, out increment);
                        float.TryParse(value, out intValue);

                        string[] rangeParts = rangeRaw.Split(new[] { ' ' });
                        if (rangeParts.Count() == 2 && increment > 0)
                        {
                            float lowerRange;
                            float.TryParse(rangeParts[0], out lowerRange);
                            float upperRange;
                            float.TryParse(rangeParts[1], out upperRange);

                            float displayscale = 1;

                        //    var rangeControl = new RangeControl();

                            if (units.ToLower() == "centi-degrees")
                            {
                                Console.WriteLine(x.Key + " scale");
                                displayscale = 100;
                                units = "Degrees (Scaled)";
                                increment /= 100;
                            } else if (units.ToLower() == "centimeters")
                            {
                                Console.WriteLine(x.Key + " scale");
                                displayscale = 100;
                                units = "Meters (Scaled)";
                                increment /= 100;
                            }

                            var rangeControl = new RangeControl(x.Key, FitDescriptionText(units, description), displayName, increment, displayscale, lowerRange, upperRange, value);

                            /*
                            rangeControl.Name = x.Key;
                            rangeControl.Increment = increment;
                            rangeControl.DescriptionText = FitDescriptionText(units, description);
                            rangeControl.LabelText = displayName;
                            rangeControl.MinRange = lowerRange;
                            rangeControl.MaxRange = upperRange;
                            rangeControl.Value = value;
                            */
                            Console.WriteLine("{0} {1} {2} {3} {4}", x.Key, increment, lowerRange, upperRange, value);

                            ThemeManager.ApplyThemeTo(rangeControl);

                            if (intValue < lowerRange)
                                rangeControl.NumericUpDownControl.BackColor = Color.Orange;

                            if (intValue > upperRange)
                                rangeControl.NumericUpDownControl.BackColor = Color.Orange;
/*
                            rangeControl.TrackBarControl.Minimum = Math.Min(scaledLowerRange, (int)intValue);
                            rangeControl.TrackBarControl.Maximum = Math.Max(scaledUpperRange, (int)intValue);
                            rangeControl.TrackBarControl.TickFrequency = scaledIncrement;
                            rangeControl.TrackBarControl.Value = (int)intValue;

                            rangeControl.NumericUpDownControl.Increment = (decimal)increment;
                            rangeControl.NumericUpDownControl.DecimalPlaces = scaler.ToString(CultureInfo.InvariantCulture).Length - 1;
                            rangeControl.NumericUpDownControl.Minimum = (decimal)Math.Min(lowerRange, intValue);
                            rangeControl.NumericUpDownControl.Maximum = (decimal)Math.Max(upperRange, intValue);
                            rangeControl.NumericUpDownControl.Value = (decimal)(intValue);
                            */
                            rangeControl.AttachEvents();

                            tableLayoutPanel1.Controls.Add(rangeControl);

                            controlAdded = true;
                        }
                    }

                    if (!controlAdded)
                    {
                        // If this is a subset of values
                        string availableValuesRaw = _parameterMetaDataRepository.GetParameterMetaData(x.Key, ParameterMetaDataConstants.Values);
                        if (!String.IsNullOrEmpty(availableValuesRaw))
                        {
                            string[] availableValues = availableValuesRaw.Split(new[] { ',' });
                            if (availableValues.Any())
                            {
                                var valueControl = new ValuesControl();
                                valueControl.Name = x.Key;
                                valueControl.DescriptionText = FitDescriptionText(units, description);
                                valueControl.LabelText = displayName;

                                ThemeManager.ApplyThemeTo(valueControl);

                                var splitValues = new List<KeyValuePair<string, string>>();
                                // Add the values to the ddl
                                foreach (string val in availableValues)
                                {
                                    string[] valParts = val.Split(new[] { ':' });
                                    splitValues.Add(new KeyValuePair<string, string>(valParts[0].Trim(), (valParts.Length > 1) ? valParts[1].Trim() : valParts[0].Trim()));
                                };
                                valueControl.ComboBoxControl.DisplayMember = "Value";
                                valueControl.ComboBoxControl.ValueMember = "Key";
                                valueControl.ComboBoxControl.DataSource = splitValues;
                                valueControl.ComboBoxControl.SelectedValue = value;

                                tableLayoutPanel1.Controls.Add(valueControl);
                            }
                        }
                    }
                } // if there is an error simply dont show it, ie bad pde file, bad scale etc
                catch (Exception ex) { log.Error(ex); }
            }
        }

        /// <summary>
        /// Fits the description text.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        private string FitDescriptionText(string description)
        {
            return FitDescriptionText(string.Empty, description);
        }

        /// <summary>
        /// Fits the description text.
        /// </summary>
        /// <param name="units">The units.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        private string FitDescriptionText(string units, string description)
        {
            var returnDescription = new StringBuilder();

            if (!String.IsNullOrEmpty(units))
            {
                returnDescription.Append(String.Format("Units: {0}{1}", units, Environment.NewLine));
            }

            if (!String.IsNullOrEmpty(description))
            {
                returnDescription.Append("Description: ");
                var descriptionParts = description.Split(new char[] { ' ' });
                for (int i = 0; i < descriptionParts.Length; i++)
                {
                    returnDescription.Append(String.Format("{0} ", descriptionParts[i]));
                    if (i != 0 && i % 12 == 0) returnDescription.Append(Environment.NewLine);
                }
            }

            return returnDescription.ToString();
        }

        #endregion
    }
}
