using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace HapticGloveServer
{

    public class GATTDefaultService
    {
        public string Description
        {
            private set;
            get;
        }

        public ushort? ID
        {
            private set;
            get;
        }

        public Guid UUID
        {
            private set;
            get;
        }

        public string Filter
        {
            private set;
            get;
        }

        private GATTDefaultService(string description, ushort id)
            : this(description, new Guid(string.Format("{{0000{0,4:X}-0000-1000-8000-00805f9b34fb}}", id)))
        {
            this.ID = id;
        }

        public GATTDefaultService(string description, Guid uuid)
        {
            this.Description = description;
            this.UUID = uuid;
            this.Filter = GattDeviceService.GetDeviceSelectorFromUuid(uuid);
            lookup.Add(this.UUID, this);
        }

        static Dictionary<Guid, GATTDefaultService> lookup;

        public static GATTDefaultService Find(Guid uuid)
        {
            if(lookup.ContainsKey(uuid))
            {
                return lookup[uuid];
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<GATTDefaultService> All
        {
            get
            {
                return lookup.Values;
            }
        }

        static GATTDefaultService()
        {
            lookup = new Dictionary<Guid, GATTDefaultService>();

            AlertNotificationService = new GATTDefaultService("Alert Notification Service", 0x1811);
            AutomationIO = new GATTDefaultService("Automation IO", 0x1815);
            BatteryService = new GATTDefaultService("Battery Service", 0x180F);
            BloodPressure = new GATTDefaultService("Blood Pressure", 0x1810);
            BodyComposition = new GATTDefaultService("Body Composition", 0x181B);
            BondManagement = new GATTDefaultService("Bond Management", 0x181E);
            ContinuousGlucoseMonitoring = new GATTDefaultService("Continuous Glucose Monitoring", 0x181F);
            CurrentTimeService = new GATTDefaultService("Current Time Service", 0x1805);
            CyclingPower = new GATTDefaultService("Cycling Power", 0x1818);
            CyclingSpeedAndCadence = new GATTDefaultService("Cycling Speed and Cadence", 0x1816);
            DeviceInformation = new GATTDefaultService("Device Information", 0x180A);
            EnvironmentalSensing = new GATTDefaultService("Environmental Sensing", 0x181A);
            GenericAccess = new GATTDefaultService("Generic Access", 0x1800);
            GenericAttribute = new GATTDefaultService("Generic Attribute", 0x1801);
            Glucose = new GATTDefaultService("Glucose", 0x1808);
            HealthThermometer = new GATTDefaultService("Health Thermometer", 0x1809);
            HeartRate = new GATTDefaultService("Heart Rate", 0x180D);
            HTTPProxy = new GATTDefaultService("HTTP Proxy", 0x1823);
            HumanInterfaceDevice = new GATTDefaultService("Human Interface Device", 0x1812);
            ImmediateAlert = new GATTDefaultService("Immediate Alert", 0x1802);
            IndoorPositioning = new GATTDefaultService("Indoor Positioning", 0x1821);
            InternetProtocolSupport = new GATTDefaultService("Internet Protocol Support", 0x1820);
            LinkLoss = new GATTDefaultService("Link Loss", 0x1803);
            LocationAndNavigation = new GATTDefaultService("Location and Navigation", 0x1819);
            NextDSTChangeService = new GATTDefaultService("Next DST Change Service", 0x1807);
            ObjectTransfer = new GATTDefaultService("Object Transfer", 0x1825);
            PhoneAlertStatusService = new GATTDefaultService("Phone Alert Status Service", 0x180E);
            PulseOximeter = new GATTDefaultService("Pulse Oximeter", 0x1822);
            ReferenceTimeUpdateService = new GATTDefaultService("Reference Time Update Service", 0x1806);
            RunningSpeedAndCadence = new GATTDefaultService("Running Speed and Cadence", 0x1814);
            ScanParameters = new GATTDefaultService("Scan Parameters", 0x1813);
            TransportDiscovery = new GATTDefaultService("Transport Discovery", 0x1824);
            TxPower = new GATTDefaultService("Tx Power", 0x1804);
            UserData = new GATTDefaultService("User Data", 0x181C);
            WeightScale = new GATTDefaultService("Weight Scale", 0x181D);
        }

        public static GATTDefaultService AlertNotificationService, AutomationIO, BatteryService, BloodPressure, BodyComposition,
            BondManagement, ContinuousGlucoseMonitoring, CurrentTimeService, CyclingPower, CyclingSpeedAndCadence, DeviceInformation,
            EnvironmentalSensing, GenericAccess, GenericAttribute, Glucose, HealthThermometer, HeartRate, HTTPProxy, HumanInterfaceDevice,
            ImmediateAlert, IndoorPositioning, InternetProtocolSupport, LinkLoss, LocationAndNavigation, NextDSTChangeService,
            ObjectTransfer, PhoneAlertStatusService, PulseOximeter, ReferenceTimeUpdateService, RunningSpeedAndCadence, ScanParameters,
            TransportDiscovery, TxPower, UserData, WeightScale;
    }
}
