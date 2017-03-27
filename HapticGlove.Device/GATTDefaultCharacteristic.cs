using System;
using System.Collections.Generic;

namespace NotionTheory.HapticGlove
{
    public class GATTDefaultCharacteristic
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

        public GATTDefaultCharacteristic(string description, Guid uuid)
        {
            this.Description = description;
            this.UUID = uuid;
            lookup.Add(this.UUID, this);
        }

        private GATTDefaultCharacteristic(string description, ushort id)
            : this(description, new Guid(string.Format("{{0000{0,4:X}-0000-1000-8000-00805f9b34fb}}", id)))
        {
            this.ID = id;
        }

        static Dictionary<Guid, GATTDefaultCharacteristic> lookup;

        public static GATTDefaultCharacteristic Find(Guid uuid)
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

        public static IEnumerable<GATTDefaultCharacteristic> All
        {
            get
            {
                return lookup.Values;
            }
        }

        static GATTDefaultCharacteristic()
        {
            lookup = new Dictionary<Guid, GATTDefaultCharacteristic>();

            AerobicHeartRateLowerLimit = new GATTDefaultCharacteristic("Aerobic Heart Rate Lower Limit", 0x2A7E);
            AerobicHeartRateUpperLimit = new GATTDefaultCharacteristic("Aerobic Heart Rate Upper Limit", 0x2A84);
            AerobicThreshold = new GATTDefaultCharacteristic("Aerobic Threshold", 0x2A7F);
            Age = new GATTDefaultCharacteristic("Age", 0x2A80);
            Aggregate = new GATTDefaultCharacteristic("Aggregate", 0x2A5A);
            AlertCategoryID = new GATTDefaultCharacteristic("Alert Category ID", 0x2A43);
            AlertCategoryIDBitMask = new GATTDefaultCharacteristic("Alert Category ID Bit Mask", 0x2A42);
            AlertLevel = new GATTDefaultCharacteristic("Alert Level", 0x2A06);
            AlertNotificationControlPoint = new GATTDefaultCharacteristic("Alert Notification Control Point", 0x2A44);
            AlertStatus = new GATTDefaultCharacteristic("Alert Status", 0x2A3F);
            Altitude = new GATTDefaultCharacteristic("Altitude", 0x2AB3);
            AnaerobicHeartRateLowerLimit = new GATTDefaultCharacteristic("Anaerobic Heart Rate Lower Limit", 0x2A81);
            AnaerobicHeartRateUpperLimit = new GATTDefaultCharacteristic("Anaerobic Heart Rate Upper Limit", 0x2A82);
            AnaerobicThreshold = new GATTDefaultCharacteristic("Anaerobic Threshold", 0x2A83);
            Analog = new GATTDefaultCharacteristic("Analog", 0x2A58);
            ApparentWindDirection = new GATTDefaultCharacteristic("Apparent Wind Direction", 0x2A73);
            ApparentWindSpeed = new GATTDefaultCharacteristic("Apparent Wind Speed", 0x2A72);
            Appearance = new GATTDefaultCharacteristic("Appearance", 0x2A01);
            BarometricPressureTrend = new GATTDefaultCharacteristic("Barometric Pressure Trend", 0x2AA3);
            BatteryLevel = new GATTDefaultCharacteristic("Battery Level", 0x2A19);
            BloodPressureFeature = new GATTDefaultCharacteristic("Blood Pressure Feature", 0x2A49);
            BloodPressureMeasurement = new GATTDefaultCharacteristic("Blood Pressure Measurement", 0x2A35);
            BodyCompositionFeature = new GATTDefaultCharacteristic("Body Composition Feature", 0x2A9B);
            BodyCompositionMeasurement = new GATTDefaultCharacteristic("Body Composition Measurement", 0x2A9C);
            BodySensorLocation = new GATTDefaultCharacteristic("Body Sensor Location", 0x2A38);
            BondManagementControlPoint = new GATTDefaultCharacteristic("Bond Management Control Point", 0x2AA4);
            BondManagementFeature = new GATTDefaultCharacteristic("Bond Management Feature", 0x2AA5);
            BootKeyboardInputReport = new GATTDefaultCharacteristic("Boot Keyboard Input Report", 0x2A22);
            BootKeyboardOutputReport = new GATTDefaultCharacteristic("Boot Keyboard Output Report", 0x2A32);
            BootMouseInputReport = new GATTDefaultCharacteristic("Boot Mouse Input Report", 0x2A33);
            CentralAddressResolution = new GATTDefaultCharacteristic("Central Address Resolution", 0x2AA6);
            CGMFeature = new GATTDefaultCharacteristic("CGM Feature", 0x2AA8);
            CGMMeasurement = new GATTDefaultCharacteristic("CGM Measurement", 0x2AA7);
            CGMSessionRunTime = new GATTDefaultCharacteristic("CGM Session Run Time", 0x2AAB);
            CGMSessionStartTime = new GATTDefaultCharacteristic("CGM Session Start Time", 0x2AAA);
            CGMSpecificOpsControlPoint = new GATTDefaultCharacteristic("CGM Specific Ops Control Point", 0x2AAC);
            CGMStatus = new GATTDefaultCharacteristic("CGM Status", 0x2AA9);
            CSCFeature = new GATTDefaultCharacteristic("CSC Feature", 0x2A5C);
            CSCMeasurement = new GATTDefaultCharacteristic("CSC Measurement", 0x2A5B);
            CurrentTime = new GATTDefaultCharacteristic("Current Time", 0x2A2B);
            CyclingPowerControlPoint = new GATTDefaultCharacteristic("Cycling Power Control Point", 0x2A66);
            CyclingPowerFeature = new GATTDefaultCharacteristic("Cycling Power Feature", 0x2A65);
            CyclingPowerMeasurement = new GATTDefaultCharacteristic("Cycling Power Measurement", 0x2A63);
            CyclingPowerVector = new GATTDefaultCharacteristic("Cycling Power Vector", 0x2A64);
            DatabaseChangeIncrement = new GATTDefaultCharacteristic("Database Change Increment", 0x2A99);
            DateofBirth = new GATTDefaultCharacteristic("Date of Birth", 0x2A85);
            DateofThresholdAssessment = new GATTDefaultCharacteristic("Date of Threshold Assessment", 0x2A86);
            DateTime = new GATTDefaultCharacteristic("Date Time", 0x2A08);
            DayDateTime = new GATTDefaultCharacteristic("Day Date Time", 0x2A0A);
            DayofWeek = new GATTDefaultCharacteristic("Day of Week", 0x2A09);
            DescriptorValueChanged = new GATTDefaultCharacteristic("Descriptor Value Changed", 0x2A7D);
            DeviceName = new GATTDefaultCharacteristic("Device Name", 0x2A00);
            DewPoint = new GATTDefaultCharacteristic("Dew Point", 0x2A7B);
            Digital = new GATTDefaultCharacteristic("Digital", 0x2A56);
            DSTOffset = new GATTDefaultCharacteristic("DST Offset", 0x2A0D);
            Elevation = new GATTDefaultCharacteristic("Elevation", 0x2A6C);
            EmailAddress = new GATTDefaultCharacteristic("Email Address", 0x2A87);
            ExactTime256 = new GATTDefaultCharacteristic("Exact Time 256", 0x2A0C);
            FatBurnHeartRateLowerLimit = new GATTDefaultCharacteristic("Fat Burn Heart Rate Lower Limit", 0x2A88);
            FatBurnHeartRateUpperLimit = new GATTDefaultCharacteristic("Fat Burn Heart Rate Upper Limit", 0x2A89);
            FirmwareRevisionString = new GATTDefaultCharacteristic("Firmware Revision String", 0x2A26);
            FirstName = new GATTDefaultCharacteristic("First Name", 0x2A8A);
            FiveZoneHeartRateLimits = new GATTDefaultCharacteristic("Five Zone Heart Rate Limits", 0x2A8B);
            FloorNumber = new GATTDefaultCharacteristic("Floor Number", 0x2AB2);
            Gender = new GATTDefaultCharacteristic("Gender", 0x2A8C);
            GlucoseFeature = new GATTDefaultCharacteristic("Glucose Feature", 0x2A51);
            GlucoseMeasurement = new GATTDefaultCharacteristic("Glucose Measurement", 0x2A18);
            GlucoseMeasurementContext = new GATTDefaultCharacteristic("Glucose Measurement Context", 0x2A34);
            GustFactor = new GATTDefaultCharacteristic("Gust Factor", 0x2A74);
            HardwareRevisionString = new GATTDefaultCharacteristic("Hardware Revision String", 0x2A27);
            HeartRateControlPoint = new GATTDefaultCharacteristic("Heart Rate Control Point", 0x2A39);
            HeartRateMax = new GATTDefaultCharacteristic("Heart Rate Max", 0x2A8D);
            HeartRateMeasurement = new GATTDefaultCharacteristic("Heart Rate Measurement", 0x2A37);
            HeatIndex = new GATTDefaultCharacteristic("Heat Index", 0x2A7A);
            Height = new GATTDefaultCharacteristic("Height", 0x2A8E);
            HIDControlPoint = new GATTDefaultCharacteristic("HID Control Point", 0x2A4C);
            HIDInformation = new GATTDefaultCharacteristic("HID Information", 0x2A4A);
            HipCircumference = new GATTDefaultCharacteristic("Hip Circumference", 0x2A8F);
            HTTPControlPoint = new GATTDefaultCharacteristic("HTTP Control Point", 0x2ABA);
            HTTPEntityBody = new GATTDefaultCharacteristic("HTTP Entity Body", 0x2AB9);
            HTTPHeaders = new GATTDefaultCharacteristic("HTTP Headers", 0x2AB7);
            HTTPStatusCode = new GATTDefaultCharacteristic("HTTP Status Code", 0x2AB8);
            HTTPSSecurity = new GATTDefaultCharacteristic("HTTPS Security", 0x2ABB);
            Humidity = new GATTDefaultCharacteristic("Humidity", 0x2A6F);
            IEEE11073_20601RegulatoryCertificationDataList = new GATTDefaultCharacteristic("20601 Regulatory Certification Data List", 0x2A2A);
            IndoorPositioningConfiguration = new GATTDefaultCharacteristic("Indoor Positioning Configuration", 0x2AAD);
            IntermediateCuffPressure = new GATTDefaultCharacteristic("Intermediate Cuff Pressure", 0x2A36);
            IntermediateTemperature = new GATTDefaultCharacteristic("Intermediate Temperature", 0x2A1E);
            Irradiance = new GATTDefaultCharacteristic("Irradiance", 0x2A77);
            Language = new GATTDefaultCharacteristic("Language", 0x2AA2);
            LastName = new GATTDefaultCharacteristic("Last Name", 0x2A90);
            Latitude = new GATTDefaultCharacteristic("Latitude", 0x2AAE);
            LNControlPoint = new GATTDefaultCharacteristic("LN Control Point", 0x2A6B);
            LNFeature = new GATTDefaultCharacteristic("LN Feature", 0x2A6A);
            LocalEastCoordinate = new GATTDefaultCharacteristic("Local East Coordinate", 0x2AB1);
            LocalNorthCoordinate = new GATTDefaultCharacteristic("Local North Coordinate", 0x2AB0);
            LocalTimeInformation = new GATTDefaultCharacteristic("Local Time Information", 0x2A0F);
            LocationandSpeed = new GATTDefaultCharacteristic("Location and Speed", 0x2A67);
            LocationName = new GATTDefaultCharacteristic("Location Name", 0x2AB5);
            Longitude = new GATTDefaultCharacteristic("Longitude", 0x2AAF);
            MagneticDeclination = new GATTDefaultCharacteristic("Magnetic Declination", 0x2A2C);
            MagneticFluxDensity2D = new GATTDefaultCharacteristic("Magnetic Flux Density - 2D", 0x2AA0);
            MagneticFluxDensity3D = new GATTDefaultCharacteristic("Magnetic Flux Density - 3D", 0x2AA1);
            ManufacturerNameString = new GATTDefaultCharacteristic("Manufacturer Name String", 0x2A29);
            MaximumRecommendedHeartRate = new GATTDefaultCharacteristic("Maximum Recommended Heart Rate", 0x2A91);
            MeasurementInterval = new GATTDefaultCharacteristic("Measurement Interval", 0x2A21);
            ModelNumberString = new GATTDefaultCharacteristic("Model Number String", 0x2A24);
            Navigation = new GATTDefaultCharacteristic("Navigation", 0x2A68);
            NewAlert = new GATTDefaultCharacteristic("New Alert", 0x2A46);
            ObjectActionControlPoint = new GATTDefaultCharacteristic("Object Action Control Point", 0x2AC5);
            ObjectChanged = new GATTDefaultCharacteristic("Object Changed", 0x2AC8);
            ObjectFirst_Created = new GATTDefaultCharacteristic("Object First-Created", 0x2AC1);
            ObjectID = new GATTDefaultCharacteristic("Object ID", 0x2AC3);
            ObjectLast_Modified = new GATTDefaultCharacteristic("Object Last-Modified", 0x2AC2);
            ObjectListControlPoint = new GATTDefaultCharacteristic("Object List Control Point", 0x2AC6);
            ObjectListFilter = new GATTDefaultCharacteristic("Object List Filter", 0x2AC7);
            ObjectName = new GATTDefaultCharacteristic("Object Name", 0x2ABE);
            ObjectProperties = new GATTDefaultCharacteristic("Object Properties", 0x2AC4);
            ObjectSize = new GATTDefaultCharacteristic("Object Size", 0x2AC0);
            ObjectType = new GATTDefaultCharacteristic("Object Type", 0x2ABF);
            OTSFeature = new GATTDefaultCharacteristic("OTS Feature", 0x2ABD);
            PeripheralPreferredConnectionParameters = new GATTDefaultCharacteristic("Peripheral Preferred Connection Parameters", 0x2A04);
            PeripheralPrivacyFlag = new GATTDefaultCharacteristic("Peripheral Privacy Flag", 0x2A02);
            PLXContinuousMeasurement = new GATTDefaultCharacteristic("PLX Continuous Measurement", 0x2A5F);
            PLXFeatures = new GATTDefaultCharacteristic("PLX Features", 0x2A60);
            PLXSpot_CheckMeasurement = new GATTDefaultCharacteristic("PLX Spot-Check Measurement", 0x2A5E);
            PnPID = new GATTDefaultCharacteristic("PnP ID", 0x2A50);
            PollenConcentration = new GATTDefaultCharacteristic("Pollen Concentration", 0x2A75);
            PositionQuality = new GATTDefaultCharacteristic("Position Quality", 0x2A69);
            Pressure = new GATTDefaultCharacteristic("Pressure", 0x2A6D);
            ProtocolMode = new GATTDefaultCharacteristic("Protocol Mode", 0x2A4E);
            Rainfall = new GATTDefaultCharacteristic("Rainfall", 0x2A78);
            ReconnectionAddress = new GATTDefaultCharacteristic("Reconnection Address", 0x2A03);
            RecordAccessControlPoint = new GATTDefaultCharacteristic("Record Access Control Point", 0x2A52);
            ReferenceTimeInformation = new GATTDefaultCharacteristic("Reference Time Information", 0x2A14);
            Report = new GATTDefaultCharacteristic("Report", 0x2A4D);
            ReportMap = new GATTDefaultCharacteristic("Report Map", 0x2A4B);
            ResolvablePrivateAddressOnly = new GATTDefaultCharacteristic("Resolvable Private Address Only", 0x2AC9);
            RestingHeartRate = new GATTDefaultCharacteristic("Resting Heart Rate", 0x2A92);
            RingerControlPoint = new GATTDefaultCharacteristic("Ringer Control Point", 0x2A40);
            RingerSetting = new GATTDefaultCharacteristic("Ringer Setting", 0x2A41);
            RSCFeature = new GATTDefaultCharacteristic("RSC Feature", 0x2A54);
            RSCMeasurement = new GATTDefaultCharacteristic("RSC Measurement", 0x2A53);
            SCControlPoint = new GATTDefaultCharacteristic("SC Control Point", 0x2A55);
            ScanIntervalWindow = new GATTDefaultCharacteristic("Scan Interval Window", 0x2A4F);
            ScanRefresh = new GATTDefaultCharacteristic("Scan Refresh", 0x2A31);
            SensorLocation = new GATTDefaultCharacteristic("Sensor Location", 0x2A5D);
            SerialNumberString = new GATTDefaultCharacteristic("Serial Number String", 0x2A25);
            ServiceChanged = new GATTDefaultCharacteristic("Service Changed", 0x2A05);
            SoftwareRevisionString = new GATTDefaultCharacteristic("Software Revision String", 0x2A28);
            SportTypeforAerobicandAnaerobicThresholds = new GATTDefaultCharacteristic("Sport Type for Aerobic and Anaerobic Thresholds", 0x2A93);
            SupportedNewAlertCategory = new GATTDefaultCharacteristic("Supported New Alert Category", 0x2A47);
            SupportedUnreadAlertCategory = new GATTDefaultCharacteristic("Supported Unread Alert Category", 0x2A48);
            SystemID = new GATTDefaultCharacteristic("System ID", 0x2A23);
            TDSControlPoint = new GATTDefaultCharacteristic("TDS Control Point", 0x2ABC);
            Temperature = new GATTDefaultCharacteristic("Temperature", 0x2A6E);
            TemperatureMeasurement = new GATTDefaultCharacteristic("Temperature Measurement", 0x2A1C);
            TemperatureType = new GATTDefaultCharacteristic("Temperature Type", 0x2A1D);
            ThreeZoneHeartRateLimits = new GATTDefaultCharacteristic("Three Zone Heart Rate Limits", 0x2A94);
            TimeAccuracy = new GATTDefaultCharacteristic("Time Accuracy", 0x2A12);
            TimeSource = new GATTDefaultCharacteristic("Time Source", 0x2A13);
            TimeUpdateControlPoint = new GATTDefaultCharacteristic("Time Update Control Point", 0x2A16);
            TimeUpdateState = new GATTDefaultCharacteristic("Time Update State", 0x2A17);
            TimewithDST = new GATTDefaultCharacteristic("Time with DST", 0x2A11);
            TimeZone = new GATTDefaultCharacteristic("Time Zone", 0x2A0E);
            TrueWindDirection = new GATTDefaultCharacteristic("True Wind Direction", 0x2A71);
            TrueWindSpeed = new GATTDefaultCharacteristic("True Wind Speed", 0x2A70);
            TwoZoneHeartRateLimit = new GATTDefaultCharacteristic("Two Zone Heart Rate Limit", 0x2A95);
            TxPowerLevel = new GATTDefaultCharacteristic("Tx Power Level", 0x2A07);
            Uncertainty = new GATTDefaultCharacteristic("Uncertainty", 0x2AB4);
            UnreadAlertStatus = new GATTDefaultCharacteristic("Unread Alert Status", 0x2A45);
            URI = new GATTDefaultCharacteristic("URI", 0x2AB6);
            UserControlPoint = new GATTDefaultCharacteristic("User Control Point", 0x2A9F);
            UserIndex = new GATTDefaultCharacteristic("User Index", 0x2A9A);
            UVIndex = new GATTDefaultCharacteristic("UV Index", 0x2A76);
            VO2Max = new GATTDefaultCharacteristic("VO2 Max", 0x2A96);
            WaistCircumference = new GATTDefaultCharacteristic("Waist Circumference", 0x2A97);
            Weight = new GATTDefaultCharacteristic("Weight", 0x2A98);
            WeightMeasurement = new GATTDefaultCharacteristic("Weight Measurement", 0x2A9D);
            WeightScaleFeature = new GATTDefaultCharacteristic("Weight Scale Feature", 0x2A9E);
            WindChill = new GATTDefaultCharacteristic("Wind Chill", 0x2A79);

            CharacteristicAggregateFormat = new GATTDefaultCharacteristic("Characteristic Aggregate Format", 0x2905);
            CharacteristicExtendedProperties = new GATTDefaultCharacteristic("Characteristic Extended Properties", 0x2900);
            CharacteristicPresentationFormat = new GATTDefaultCharacteristic("Characteristic Presentation Format", 0x2904);
            CharacteristicUserDescription = new GATTDefaultCharacteristic("Characteristic User Description", 0x2901);
            ClientCharacteristicConfiguration = new GATTDefaultCharacteristic("Client Characteristic Configuration", 0x2902);
            EnvironmentalSensingConfiguration = new GATTDefaultCharacteristic("Environmental Sensing Configuration", 0x290B);
            EnvironmentalSensingMeasurement = new GATTDefaultCharacteristic("Environmental Sensing Measurement", 0x290C);
            EnvironmentalSensingTriggerSetting = new GATTDefaultCharacteristic("Environmental Sensing Trigger Setting", 0x290D);
            ExternalReportReference = new GATTDefaultCharacteristic("External Report Reference", 0x2907);
            NumberofDigitals = new GATTDefaultCharacteristic("Number of Digitals", 0x2909);
            ReportReference = new GATTDefaultCharacteristic("Report Reference", 0x2908);
            ServerCharacteristicConfiguration = new GATTDefaultCharacteristic("Server Characteristic Configuration", 0x2903);
            TimeTriggerSetting = new GATTDefaultCharacteristic("Time Trigger Setting", 0x290E);
            ValidRange = new GATTDefaultCharacteristic("Valid Range", 0x2906);
            ValueTriggerSetting = new GATTDefaultCharacteristic("Value Trigger Setting", 0x290A);

        }

        public static GATTDefaultCharacteristic AerobicHeartRateLowerLimit, AerobicHeartRateUpperLimit,
            AerobicThreshold, Age, Aggregate, AlertCategoryID, AlertCategoryIDBitMask, AlertLevel,
            AlertNotificationControlPoint, AlertStatus, Altitude, AnaerobicHeartRateLowerLimit,
            AnaerobicHeartRateUpperLimit, AnaerobicThreshold, Analog, ApparentWindDirection,
            ApparentWindSpeed, Appearance, BarometricPressureTrend, BatteryLevel, BloodPressureFeature,
            BloodPressureMeasurement, BodyCompositionFeature, BodyCompositionMeasurement, BodySensorLocation,
            BondManagementControlPoint, BondManagementFeature, BootKeyboardInputReport, BootKeyboardOutputReport,
            BootMouseInputReport, CentralAddressResolution, CGMFeature, CGMMeasurement, CGMSessionRunTime,
            CGMSessionStartTime, CGMSpecificOpsControlPoint, CGMStatus, CSCFeature, CSCMeasurement,
            CurrentTime, CyclingPowerControlPoint, CyclingPowerFeature, CyclingPowerMeasurement,
            CyclingPowerVector, DatabaseChangeIncrement, DateofBirth, DateofThresholdAssessment, DateTime,
            DayDateTime, DayofWeek, DescriptorValueChanged, DeviceName, DewPoint, Digital, DSTOffset,
            Elevation, EmailAddress, ExactTime256, FatBurnHeartRateLowerLimit, FatBurnHeartRateUpperLimit,
            FirmwareRevisionString, FirstName, FiveZoneHeartRateLimits, FloorNumber, Gender, GlucoseFeature,
            GlucoseMeasurement, GlucoseMeasurementContext, GustFactor, HardwareRevisionString, HeartRateControlPoint,
            HeartRateMax, HeartRateMeasurement, HeatIndex, Height, HIDControlPoint, HIDInformation, HipCircumference,
            HTTPControlPoint, HTTPEntityBody, HTTPHeaders, HTTPStatusCode, HTTPSSecurity, Humidity,
            IEEE11073_20601RegulatoryCertificationDataList, IndoorPositioningConfiguration, IntermediateCuffPressure,
            IntermediateTemperature, Irradiance, Language, LastName, Latitude, LNControlPoint, LNFeature,
            LocalEastCoordinate, LocalNorthCoordinate, LocalTimeInformation, LocationandSpeed, LocationName,
            Longitude, MagneticDeclination, MagneticFluxDensity2D, MagneticFluxDensity3D, ManufacturerNameString,
            MaximumRecommendedHeartRate, MeasurementInterval, ModelNumberString, Navigation, NewAlert,
            ObjectActionControlPoint, ObjectChanged, ObjectFirst_Created, ObjectID, ObjectLast_Modified,
            ObjectListControlPoint, ObjectListFilter, ObjectName, ObjectProperties, ObjectSize, ObjectType,
            OTSFeature, PeripheralPreferredConnectionParameters, PeripheralPrivacyFlag, PLXContinuousMeasurement,
            PLXFeatures, PLXSpot_CheckMeasurement, PnPID, PollenConcentration, PositionQuality, Pressure,
            ProtocolMode, Rainfall, ReconnectionAddress, RecordAccessControlPoint, ReferenceTimeInformation,
            Report, ReportMap, ResolvablePrivateAddressOnly, RestingHeartRate, RingerControlPoint,
            RingerSetting, RSCFeature, RSCMeasurement, SCControlPoint, ScanIntervalWindow, ScanRefresh,
            SensorLocation, SerialNumberString, ServiceChanged, SoftwareRevisionString, SportTypeforAerobicandAnaerobicThresholds,
            SupportedNewAlertCategory, SupportedUnreadAlertCategory, SystemID, TDSControlPoint, Temperature,
            TemperatureMeasurement, TemperatureType, ThreeZoneHeartRateLimits, TimeAccuracy, TimeSource,
            TimeUpdateControlPoint, TimeUpdateState, TimewithDST, TimeZone, TrueWindDirection, TrueWindSpeed,
            TwoZoneHeartRateLimit, TxPowerLevel, Uncertainty, UnreadAlertStatus, URI, UserControlPoint, UserIndex,
            UVIndex, VO2Max, WaistCircumference, Weight, WeightMeasurement, WeightScaleFeature, WindChill,


            CharacteristicAggregateFormat, CharacteristicExtendedProperties, CharacteristicPresentationFormat,
            CharacteristicUserDescription, ClientCharacteristicConfiguration, EnvironmentalSensingConfiguration,
            EnvironmentalSensingMeasurement, EnvironmentalSensingTriggerSetting, ExternalReportReference, NumberofDigitals,
            ReportReference, ServerCharacteristicConfiguration, TimeTriggerSetting, ValidRange, ValueTriggerSetting;
    }
}
