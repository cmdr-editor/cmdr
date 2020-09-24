using System.Linq;
using cmdr.TsiLib.Commands;
using cmdr.TsiLib.Conditions;
using cmdr.TsiLib.Enums;
using cmdr.TsiLib.MidiDefinitions.Base;
using cmdr.TsiLib.MidiDefinitions;
using cmdr.TsiLib.Commands.Interpretation;

namespace cmdr.TsiLib
{
    public class Mapping
    {
        private readonly Format.Mapping _rawMapping;
        internal Format.Mapping RawMapping { get { return _rawMapping; } }

        public int Id { get { return _rawMapping.MidiNoteBindingId; } }

        public ACommand Command { get; private set; }

        public AMidiDefinition MidiBinding { get; private set; }

        public string Comment
        {
            get { return _rawMapping.Settings.Comment; }
            set { _rawMapping.Settings.Comment = value; }
        }

        public bool Blend
        {
            get
            {
                return _rawMapping.Settings.LedBlend;
            }
        }

        private readonly ConditionTuple _conditions;
        public ConditionTuple Conditions { get { return _conditions; } }

        // only for proprietary controllers and default mappings
        public bool CanOverrideFactoryMap { get; private set; }

        public bool OverrideFactoryMap
        {
            get { return !_rawMapping.Settings.UseFactoryMap; }
            set { _rawMapping.Settings.UseFactoryMap = !value; }
        }


        #region Constructors

        internal Mapping(CommandProxy command)
        {
            _rawMapping = new Format.Mapping(command.MappingType, command.Description.Id);
            Command = command.Create(_rawMapping.Settings);
            _conditions = new ConditionTuple();
        }

        internal Mapping(Device device, Format.Mapping rawMapping)
            : this(rawMapping)
        {
            if(Command == null) {  //pestrela: favorites crash
                return;
            }

            Attach(device);

            if (Id >= 0)
            {
                Format.MidiDefinition rawDefinition = getMidiDefinition(device, Command.MappingType, Id);
                if (rawDefinition != null)
                    MidiBinding = AMidiDefinition.Parse(device.TypeStr, Command.MappingType, rawDefinition);
            }
        }

        /// <summary>
        /// Private constructor. Used for copying and as base constructor.
        /// </summary>
        /// <param name="rawMapping"></param>
        private Mapping(Format.Mapping rawMapping)
        {
            _rawMapping = rawMapping;

            bool originalHasValueUI = _rawMapping.Settings.HasValueUI;

            Command = Commands.All.GetCommandProxy(_rawMapping.TraktorControlId, _rawMapping.Type).Create(_rawMapping.Settings);
            if (Command == null)
            {
                return; // null;   ////pestrela: favorites crash
            }

            if (originalHasValueUI != _rawMapping.Settings.HasValueUI)
            {
                // TODO: Something for the consolidation function
            }

            _conditions = new ConditionTuple(
                (_rawMapping.Settings.ConditionOneId > 0) ? cmdr.TsiLib.Conditions.All.GetConditionProxy(_rawMapping.Settings.ConditionOneId).Create(_rawMapping.Settings, ConditionNumber.One) : null,
                (_rawMapping.Settings.ConditionTwoId > 0) ? cmdr.TsiLib.Conditions.All.GetConditionProxy(_rawMapping.Settings.ConditionTwoId).Create(_rawMapping.Settings, ConditionNumber.Two) : null
                );
        }

        #endregion


        /// <summary>
        /// Set condition by proxy.
        /// </summary>
        /// <param name="number">Number of condition</param>
        /// <param name="proxy">ConditionProxy or null to reset condition.</param>
        /// <returns>True if condition was changed.</returns>
        public bool SetCondition(ConditionNumber number, ConditionProxy proxy)
        {
            return _conditions.SetCondition(_rawMapping.Settings, number, proxy);
        }

        /// <summary>
        /// Set condition by another condition.
        /// </summary>
        /// <param name="number">Number of condition</param>
        /// <param name="condition">ACondition or null to reset condition.</param>
        /// <returns>True if condition was changed.</returns>
        public bool SetCondition(ConditionNumber number, ACondition condition)
        {
            return _conditions.SetCondition(_rawMapping.Settings, number, condition);   
        }

        /// <summary>
        /// Set binding.
        /// </summary>
        /// <param name="device">The device this mapping belongs to. </param>
        /// <param name="midi">AMidiDefinition or null to reset binding.</param>
        /// <returns>True if binding was changed.</returns>
        public bool SetBinding(Device device, AMidiDefinition midi) // if the Device paramater was left out, it would be possible to make an invalid binding.
        {
            // given device and definition do not match. binding cannot be kept.
            if (midi != null && device.TypeStr != midi.DeviceTypeStr)
            {
                MidiBinding = null;
                return true;
            }

            // midi definitions and bindings
            var deviceData = device.RawDevice.Data;
            var definitions = (Command.MappingType == MappingType.In) ? deviceData.MidiDefinitions.In.Definitions : deviceData.MidiDefinitions.Out.Definitions;
            var bindings = deviceData.Mappings.MidiBindings.Bindings;

            // remove old binding and definition(s) if no longer in use
            if (MidiBinding != null)
            {
                // remove old binding and check if old definition can be removed too, because it is not used in another binding
                var oldBinding = bindings.SingleOrDefault(b => b.BindingId.Equals(Id));
                if (oldBinding != null)
                {
                    bindings.Remove(oldBinding);

                    var oldBindings = bindings.Where(b => b.MidiNote.Equals(MidiBinding.Note));
                    if (!oldBindings.Any())
                    {
                        int removedCount = definitions.RemoveAll(d => d.MidiNote.Equals(MidiBinding.Note));
                        if (removedCount > 1)
                        {
                            // TODO: Something for the consolidation function
                        }
                    }
                }
            }

            if (midi != null)
            {
                // add midi definition to device, if it doesn't already exist
                var matchingDefinitions = definitions.Where(d => d.MidiNote.Equals(midi.Note));
                if (matchingDefinitions.Any() == false)
                {
                    var definition = midi.RawDefinition;

                    // adapt encoder mode to device, if necessary
                    var genericDefinition = midi as AGenericMidiDefinition;
                    if (genericDefinition != null) //  && genericDefinition.MidiEncoderMode != device.EncoderMode)
                    {
                        genericDefinition = AGenericMidiDefinition.Parse(genericDefinition.Type, definition);
                        // genericDefinition.MidiEncoderMode = device.EncoderMode;
                        genericDefinition.MidiEncoderMode =
                            //this.MidiBinding.RawDefinition.MidiEncoderMode;
                            this.Command.RawSettings.EncoderMode2;

                        //genericDefinition.MidiEncoderMode = this.Command.Control.EncoderMode;

                        definition = genericDefinition.RawDefinition;  // pestrela: this was unchanged
                    }

                    definitions.Add(definition);
                } else
                {
                    // definition already exist. Update encoder 
                    //matchingDefinitions.First().EncoderMode = this.MidiBinding.MidiEncoderMode;
                }

                // add midi binding to device
                bindings.Add(new Format.MidiNoteBinding(Id, midi.Note));
            }

            // set midi binding to mapping
            bool changed = (MidiBinding != null && !MidiBinding.Equals(midi)) || (midi != null && !midi.Equals(MidiBinding));
            if (changed)
                MidiBinding = midi;

            return changed;
        }

        /// <summary>
        /// Creates a copy of this mapping.
        /// </summary>
        /// <param name="includeMidiBinding">Optionally include the midi binding.</param>
        /// <returns>Copy of mapping</returns>
        public Mapping Copy(bool includeMidiBinding)
        {
            Format.Mapping rawMappingCopy;
            using (var copyStream = new System.IO.MemoryStream())
            {
                _rawMapping.Write(new Utils.Writer(copyStream));
                copyStream.Seek(0, System.IO.SeekOrigin.Begin);
                rawMappingCopy = new Format.Mapping(copyStream);
            }

            var copy = new Mapping(rawMappingCopy);
            if (includeMidiBinding)
                copy.MidiBinding = MidiBinding;

            return copy;
        }

        public override bool Equals(object obj)
        {
            Mapping other = obj as Mapping;
            if (other == null)
                return false;

            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        internal void Attach(Device device)
        {
            CanOverrideFactoryMap = (
               device.TypeStr != Device.TYPE_STRING_GENERIC_MIDI && device.ProprietaryControllerDeviceType == Proprietary_Controller_DeviceType.Default
               );
        }

        public void hack_modifier(KnownCommands new_id)
        {
            RawMapping.TraktorControlId = (int)new_id;
        }


        private Format.MidiDefinition getMidiDefinition(Device device, MappingType type, int id)
        {
            var deviceData = device.RawDevice.Data;
            var definitions = (Command.MappingType == MappingType.In) ? deviceData.MidiDefinitions.In.Definitions : deviceData.MidiDefinitions.Out.Definitions;
            var bindings = deviceData.Mappings.MidiBindings.Bindings;

            var rawBinding = bindings.SingleOrDefault(b => b.BindingId.Equals(id));
            if (rawBinding != null)
            {
                var defs = definitions.Where(d => d.MidiNote.Equals(rawBinding.MidiNote));
                if (defs.Any())
                {
                    if (defs.Count() == 1)
                        return defs.Single();
                    else // definition duplicates found
                    {
                        // this should not happen actually, but there seems to be a bug in 
                        // Xtreme Mapping / Traktor Controller Manager.
                        // TODO: Something for the consolidation function
                        return defs.First();
                    }
                }
            }
            return null;
        }
    }
}
