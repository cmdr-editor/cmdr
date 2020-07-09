using System;
using System.Collections.Generic;
using System.Linq;
using cmdr.TsiLib.Commands;
using cmdr.TsiLib.Enums;
using cmdr.TsiLib.Format;
using cmdr.TsiLib.MidiDefinitions.Base;
using cmdr.TsiLib.Controls.Encoder;
 
namespace cmdr.TsiLib
{
    public class Device
    {
        public static readonly string TYPE_STRING_GENERIC_MIDI = "Generic MIDI";

        public bool IsKeyboard
        {
            get
            {
                return RawDevice.IsKeyboard;
            }
            set
            {
                RawDevice.IsKeyboard = value;
            }
        }

        internal Format.Device RawDevice;

        /// <summary>
        /// Internal ID that is only used when added to TsiFile (simple enumeration according to order, not persistent).
        /// Don't forget to set Id in AddDevice method of TsiFile!
        /// </summary>
        public int Id { get; internal set; }

        public int Revision { get { return RawDevice.Data.Version.MappingFileRevision; } }

        public string TypeStr
        {
            get { return RawDevice.DeviceType; }
        }

        public DeviceTarget Target
        {
            get { return RawDevice.Data.Target.DeviceTarget; }
            set { RawDevice.Data.Target.DeviceTarget = value; }
        }

        public string TraktorVersion
        {
            get { return RawDevice.Data.Version.Version; }
            set { RawDevice.Data.Version.Version = value; }
        }

        public string Comment
        {
            get { return (RawDevice.Data.Comment != null) ? RawDevice.Data.Comment.Comment : String.Empty; }
            set
            {
                if (RawDevice.Data.Comment == null)
                    RawDevice.Data.Comment = new Format.MappingFileComment();
                RawDevice.Data.Comment.Comment = value;
            }
        }

        /// <summary>
        /// Name of MIDI input device. Empty String means "All Ports".
        /// </summary>
        public string InPort
        {
            get { return RawDevice.Data.Ports.InPortName; }
            set { RawDevice.Data.Ports.InPortName = value; }
        }

        /// <summary>
        /// Name of MIDI output device. Empty String means "All Ports".
        /// </summary>
        public string OutPort
        {
            get { return RawDevice.Data.Ports.OutPortName; }
            set { RawDevice.Data.Ports.OutPortName = value; }
        }

        private List<Mapping> _mappings = new List<Mapping>();
        public IReadOnlyCollection<Mapping> Mappings { get { return _mappings.AsReadOnly(); } }

        public static bool isGenericMidiDevice(String deviceTypeStr)
        {

            if (
                (deviceTypeStr == "Traktor.Kontrol S4 MK3") ||
                (deviceTypeStr == "Traktor.Kontrol S2 MK3") ||
                (deviceTypeStr == "Traktor.Kontrol S3") ||
                // (deviceTypeStr == "Traktor.Kontrol S5") ||      // if this ever comes
                (deviceTypeStr == "Traktor.Kontrol S8") ||
                (deviceTypeStr == "Traktor.Kontrol D2") ||
                (deviceTypeStr == "Pioneer.DDJ-T1") ||
                (deviceTypeStr == "Pioneer.DJM-T1") ||
                (deviceTypeStr == "Generic Keyboard") ||
                false
                ) {
                return false;
            } else {
                return true;
            }
        }


        public Proprietary_Controller_DeviceType ProprietaryControllerDeviceType
        {
            get
            {
                // pestrela: this is to support CanOverrideFactoryMap for the new devices
                var deviceTypeStr = TypeStr;
                if (!Device.isGenericMidiDevice(deviceTypeStr))
                    return Proprietary_Controller_DeviceType.Default;
                

                if (TypeStr.EndsWith(".Default"))
                    return Proprietary_Controller_DeviceType.Default;
                else
                    return Proprietary_Controller_DeviceType.User;
            }
        }

        private Dictionary<string, AMidiDefinition> _midiInDefinitions = new Dictionary<string, AMidiDefinition>();
        public IReadOnlyDictionary<string, AMidiDefinition> MidiInDefinitions { get { return _midiInDefinitions; } }

        private Dictionary<string, AMidiDefinition> _midiOutDefinitions = new Dictionary<string, AMidiDefinition>();
        public IReadOnlyDictionary<string, AMidiDefinition> MidiOutDefinitions { get { return _midiOutDefinitions; } }

        // In Controller Manager, the encoder mode is shown among elements of MappingSettings, but not stored with them. Instead, it's stored in the MidiDefinition.
        // As the encoder mode is the same for all encoders of a controller, it should be handled by the device.
        // pestrela 2020-01-10: moving this back to midi device
        /// <summary>
        /// Encoder mode, specific to a controller and uniform for all of its encoders. Either 3Fh/41h or 7Fh/01h. Only used for generic midi devices.
        /// </summary>
        private MidiEncoderMode _encoderMode;
        public MidiEncoderMode EncoderMode
        {
            get { return _encoderMode; }
            set { _encoderMode = value; setEncoderModes(); }
        }

        public bool RemoveUnusedMIDIDefinitions { get; set; }

        internal Device(int id, string deviceTypeStr, string traktorVersion, bool removeUnusedMIDIDefinition, bool isKeyboard)
            : this(id, new Format.Device(deviceTypeStr, traktorVersion, removeUnusedMIDIDefinition, isKeyboard), removeUnusedMIDIDefinition, isKeyboard)
        {
            // workaround for Xtreme Mapping only: midi definitions must not be null!
            RawDevice.Data.MidiDefinitions = new MidiDefinitionsContainer();
        }

        internal Device(int id, Format.Device rawDevice, bool removeUnusedMIDIDefinitions, bool isKeyboard)
        {
            RemoveUnusedMIDIDefinitions = removeUnusedMIDIDefinitions;
            Id = id;
            RawDevice = rawDevice;

            IsKeyboard = isKeyboard;

            if (RawDevice.Data.Mappings != null)
                _mappings = RawDevice.Data.Mappings.List.Mappings.Select(m => new Mapping(this, m)).ToList();

            if(RemoveUnusedMIDIDefinitions)
                reduceDefinitions();

            updateMidiDefinitions();

            updateEncoderMode();

        }


        public Mapping CreateMapping(CommandProxy command)
        {
            return new Mapping(command);
        }

        public void AddMapping(Mapping mapping)
        {
            InsertMapping(Mappings.Count, mapping);
        }

        public void InsertMapping(int index, Mapping mapping)
        {
            insertMapping(index, mapping, false);
        }

        public void MoveMapping(int oldIndex, int newIndex)
        {
            var temp = _mappings[oldIndex];
            removeMapping(temp, true);
            insertMapping(newIndex, temp, true);
        }

        public void RemoveMapping(int id)
        {
            var old = _mappings.Single(m => m.Id == id);
            removeMapping(old, false);
        }

        /// <summary>
        /// Increment MappingFileRevision of device. Int32 Overflow is internally avoided by modulo.
        /// </summary>
        public void IncrementRevision()
        {
            RawDevice.Data.Version.MappingFileRevision = (RawDevice.Data.Version.MappingFileRevision + 1) % int.MaxValue;
        }

        public Device Copy(bool includeMappings)
        {
            Format.Device rawDeviceCopy;
            using (var copyStream = new System.IO.MemoryStream())
            {
                RawDevice.Write(new Utils.Writer(copyStream));
                copyStream.Seek(0, System.IO.SeekOrigin.Begin);
                rawDeviceCopy = new Format.Device(copyStream);
            }

            // reset revision
            rawDeviceCopy.Data.Version.MappingFileRevision = 0;
            
            if (!includeMappings)
                rawDeviceCopy.Data.Mappings = new MappingsContainer();

            var copy = new Device(-1, rawDeviceCopy, RemoveUnusedMIDIDefinitions, this.IsKeyboard);
            copy.EncoderMode = EncoderMode; // encoder mode is not stored in Format.Device
            return copy;
        }


        private void insertMapping(int index, Mapping mapping, bool asIs)
        {
            if (RawDevice.Data.Mappings == null)
                RawDevice.Data.Mappings = new Format.MappingsContainer();

            if (!asIs)
            {
                mapping.Attach(this);

                //mapping.RawMapping.Settings.DeviceType = Type;
                mapping.RawMapping.MidiNoteBindingId = createNewId(); // set correct id no matter if mapping is newly created or not

                // keep midi binding
                if (mapping.MidiBinding != null)
                    mapping.SetBinding(this, mapping.MidiBinding);
            }

            if (index == Mappings.Count)
            {
                RawDevice.Data.Mappings.List.Mappings.Add(mapping.RawMapping);
                _mappings.Add(mapping);
            }
            else
            {
                RawDevice.Data.Mappings.List.Mappings.Insert(index, mapping.RawMapping);
                _mappings.Insert(index, mapping);
            }
        }

        private void removeMapping(int index, bool keepBinding)
        {
            removeMapping(_mappings[index], keepBinding);
        }

        /// <summary>
        /// removes a mapping and optionally corresponding binding
        /// </summary>
        /// <param name="mapping"></param>
        private void removeMapping(Mapping mapping, bool keepBinding)
        {
            _mappings.Remove(mapping);
            var rawMappings = RawDevice.Data.Mappings.List.Mappings;
            rawMappings.Remove(mapping.RawMapping);
            if (!keepBinding)
                removeBinding(mapping.RawMapping.MidiNoteBindingId);
        }

        /// <summary>
        /// Removes the given binding and optionally its corresponding definition, if it is not used any more,
        /// </summary>
        /// <param name="bindingId">Id of binding to remove</param>
        /// <param name="definition">Optional. Remove definition if not used by another binding.</param>
        private void removeBinding(int bindingId, AMidiDefinition definition = null)
        {
            var deviceData = RawDevice.Data;
            var bindings = deviceData.Mappings.MidiBindings.Bindings;

            // remove old binding and check if old definition can be removed too, because it is not used in another binding
            var oldBinding = bindings.SingleOrDefault(b => b.BindingId.Equals(bindingId));
            if (oldBinding != null)
            {
                bindings.Remove(oldBinding);

                if (definition != null)
                {
                    var oldBindings = bindings.Where(b => b.MidiNote.Equals(definition.Note));
                    if (!oldBindings.Any())
                        removeDefinition(definition);
                }
            }
        }

        private void removeDefinition(AMidiDefinition definition)
        {
            var deviceData = RawDevice.Data;
            var definitions = (definition.Type == MappingType.In) ? deviceData.MidiDefinitions.In.Definitions : deviceData.MidiDefinitions.Out.Definitions;
            int removedCount = definitions.RemoveAll(d => d.MidiNote.Equals(definition.Note));
            if (removedCount > 0)
            {
                if (definition.Type == MappingType.In)
                    _midiInDefinitions.Remove(definition.Note);
                else
                    _midiOutDefinitions.Remove(definition.Note);
            }
            
            if (removedCount > 1)
            {
                // TODO: Something for the consolidation function
            }
        }

        private int createNewId()
        {
            var mappings = RawDevice.Data.Mappings.List.Mappings;
            int currMaxId = mappings.Any() ? mappings.Max(b => b.MidiNoteBindingId) : 0;
            if (currMaxId < int.MaxValue)
                return currMaxId + 1;
            else
            {
                // search for first unused id
                for (int i = 0; i < int.MaxValue; i++)
                {
                    if (mappings.Any(m => m.MidiNoteBindingId == i))
                        continue;
                    return i;
                }
            }
            throw new Exception("Mappings are full!");
        }

        private void updateMidiDefinitions()
        {
            var definitions = RawDevice.Data.MidiDefinitions;
            if (definitions != null)
            {
                foreach (var inDef in definitions.In.Definitions)
                    _midiInDefinitions[inDef.MidiNote] = AMidiDefinition.Parse(TypeStr, MappingType.In, inDef);

                foreach (var outDef in definitions.Out.Definitions)
                    _midiOutDefinitions[outDef.MidiNote] = AMidiDefinition.Parse(TypeStr, MappingType.Out, outDef);
            }
        }

        private void reduceDefinitions()
        {
            if (TypeStr != TYPE_STRING_GENERIC_MIDI)
                return;

            reduceDefinitions2(MappingType.In);
            reduceDefinitions2(MappingType.Out);

        }

        // note: the unused default entries are PER DEVICE
        private void reduceDefinitions2(MappingType what)
        {
            var deviceData = RawDevice.Data;
            var used_bindings = this.Mappings.Select(d => d.MidiBinding).Where(e => e != null).Where(d => d.Type == what).Select(d => d.Note).Distinct().ToList();
            List<MidiDefinition> cur_definitions = (what == MappingType.In) ? deviceData.MidiDefinitions.In.Definitions : deviceData.MidiDefinitions.Out.Definitions;

            List<MidiDefinition> new_definitions = new List<MidiDefinition>(); // = definitions.Where(d => false);   // just to get the structure

            foreach (var binding in used_bindings)
            {
                var used_definitions = cur_definitions.Where(d => d.MidiNote == binding);
                if (used_definitions.Any()) {
                    // FIXME: see what we lose here (colisions)
                    // FIXME: do this whole thing in pure LINQ
                    
                    // FIXME: also apply this at save time. Right now it is load time only.

                    new_definitions.Add(used_definitions.First());   
     
                }

            }

            // todo: remove colisions
            if (what == MappingType.In)
            {
                RawDevice.Data.MidiDefinitions.In.Definitions = new_definitions;
            } else
            {
                RawDevice.Data.MidiDefinitions.Out.Definitions = new_definitions;
            }
        }
     

        // pestrela: this is for the whole device. The new behaviour is per mapping (=per midibinding)
        private void updateEncoderMode()
        {
            if (TypeStr != TYPE_STRING_GENERIC_MIDI)
                return;

            //_encoderMode = _mappings
            //    .Where(m => m.Command.Control is EncoderControl && m.MidiBinding != null)
            //   .Select(m => (m.MidiBinding as AGenericMidiDefinition).MidiEncoderMode)
            //    .FirstOrDefault();

            var all_mappings = _mappings
                .Where(m => m.Command.Control is EncoderControl && m.MidiBinding != null)
                .Select(m => (m.MidiBinding as AGenericMidiDefinition));

            var all_encoder_modes = all_mappings
                .Select(m => m.MidiEncoderMode);

            bool multiple_modes = all_encoder_modes.Count() > 1;
            _encoderMode = all_encoder_modes.FirstOrDefault();
            return;

        }

        private void setEncoderModes()
        {
            if (TypeStr != TYPE_STRING_GENERIC_MIDI)
                return;

            // this is when we change the encoder mode on the device settings page. This is now deprecated.
            //return;

            foreach (var def in _midiInDefinitions.Values.Cast<AGenericMidiDefinition>())
                def.MidiEncoderMode = EncoderMode;
        }
    }
}
