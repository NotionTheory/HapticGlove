using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace HapticGloveServer
{

    public class GATTDefaultService
    {

        public static string MakeGATTFilter(Guid guid)
        {
            return GattDeviceService.GetDeviceSelectorFromUuid(guid);
        }

        private static string MakeGATTFilter(ushort value)
        {
            return GattDeviceService.GetDeviceSelectorFromShortId(value);
        }

        public string Description
        {
            private set;
            get;
        }

        public string Filter
        {
            private set;
            get;
        }

        public GATTDefaultService(string description, string filter)
        {
            this.Description = description;
            this.Filter = filter;
        }

        static GATTDefaultService()
        {
            AlertNotificationService = new GATTDefaultService("Alert Notification Service", MakeGATTFilter(0x1811));
            AutomationIO = new GATTDefaultService("Automation IO", MakeGATTFilter(0x1815));
            BatteryService = new GATTDefaultService("Battery Service", MakeGATTFilter(0x180F));
            BloodPressure = new GATTDefaultService("Blood Pressure", MakeGATTFilter(0x1810));
            BodyComposition = new GATTDefaultService("Body Composition", MakeGATTFilter(0x181B));
            BondManagement = new GATTDefaultService("Bond Management", MakeGATTFilter(0x181E));
            ContinuousGlucoseMonitoring = new GATTDefaultService("Continuous Glucose Monitoring", MakeGATTFilter(0x181F));
            CurrentTimeService = new GATTDefaultService("Current Time Service", MakeGATTFilter(0x1805));
            CyclingPower = new GATTDefaultService("Cycling Power", MakeGATTFilter(0x1818));
            CyclingSpeedAndCadence = new GATTDefaultService("Cycling Speed and Cadence", MakeGATTFilter(0x1816));
            DeviceInformation = new GATTDefaultService("Device Information", MakeGATTFilter(0x180A));
            EnvironmentalSensing = new GATTDefaultService("Environmental Sensing", MakeGATTFilter(0x181A));
            GenericAccess = new GATTDefaultService("Generic Access", MakeGATTFilter(0x1800));
            GenericAttribute = new GATTDefaultService("Generic Attribute", MakeGATTFilter(0x1801));
            Glucose = new GATTDefaultService("Glucose", MakeGATTFilter(0x1808));
            HealthThermometer = new GATTDefaultService("Health Thermometer", MakeGATTFilter(0x1809));
            HeartRate = new GATTDefaultService("Heart Rate", MakeGATTFilter(0x180D));
            HTTPProxy = new GATTDefaultService("HTTP Proxy", MakeGATTFilter(0x1823));
            HumanInterfaceDevice = new GATTDefaultService("Human Interface Device", MakeGATTFilter(0x1812));
            ImmediateAlert = new GATTDefaultService("Immediate Alert", MakeGATTFilter(0x1802));
            IndoorPositioning = new GATTDefaultService("Indoor Positioning", MakeGATTFilter(0x1821));
            InternetProtocolSupport = new GATTDefaultService("Internet Protocol Support", MakeGATTFilter(0x1820));
            LinkLoss = new GATTDefaultService("Link Loss", MakeGATTFilter(0x1803));
            LocationAndNavigation = new GATTDefaultService("Location and Navigation", MakeGATTFilter(0x1819));
            NextDSTChangeService = new GATTDefaultService("Next DST Change Service", MakeGATTFilter(0x1807));
            ObjectTransfer = new GATTDefaultService("Object Transfer", MakeGATTFilter(0x1825));
            PhoneAlertStatusService = new GATTDefaultService("Phone Alert Status Service", MakeGATTFilter(0x180E));
            PulseOximeter = new GATTDefaultService("Pulse Oximeter", MakeGATTFilter(0x1822));
            ReferenceTimeUpdateService = new GATTDefaultService("Reference Time Update Service", MakeGATTFilter(0x1806));
            RunningSpeedAndCadence = new GATTDefaultService("Running Speed and Cadence", MakeGATTFilter(0x1814));
            ScanParameters = new GATTDefaultService("Scan Parameters", MakeGATTFilter(0x1813));
            TransportDiscovery = new GATTDefaultService("Transport Discovery", MakeGATTFilter(0x1824));
            TxPower = new GATTDefaultService("Tx Power", MakeGATTFilter(0x1804));
            UserData = new GATTDefaultService("User Data", MakeGATTFilter(0x181C));
            WeightScale = new GATTDefaultService("Weight Scale", MakeGATTFilter(0x181D));
        }

        public static GATTDefaultService AlertNotificationService, AutomationIO, BatteryService, BloodPressure, BodyComposition, 
            BondManagement, ContinuousGlucoseMonitoring, CurrentTimeService, CyclingPower, CyclingSpeedAndCadence, DeviceInformation, 
            EnvironmentalSensing, GenericAccess, GenericAttribute, Glucose, HealthThermometer, HeartRate, HTTPProxy, HumanInterfaceDevice, 
            ImmediateAlert, IndoorPositioning, InternetProtocolSupport, LinkLoss, LocationAndNavigation, NextDSTChangeService, 
            ObjectTransfer, PhoneAlertStatusService, PulseOximeter, ReferenceTimeUpdateService, RunningSpeedAndCadence, ScanParameters, 
            TransportDiscovery, TxPower, UserData, WeightScale;
    }
}
