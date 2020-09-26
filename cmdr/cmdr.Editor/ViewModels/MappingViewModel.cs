using ChangeTracking;
using cmdr.Editor.Metadata;
using cmdr.Editor.ViewModels.Reports;
using cmdr.TsiLib;
using cmdr.TsiLib.Commands;
using cmdr.TsiLib.Conditions;
using cmdr.TsiLib.Enums;
using cmdr.TsiLib.MidiDefinitions.Base;
using cmdr.TsiLib.Commands.Interpretation;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace cmdr.Editor.ViewModels
{
    public class MappingViewModel : AReversible
    {
        private const int VALUE_TEXT_MAX_PRECISION = 4;

        private readonly Device _device;
        private readonly Mapping _mapping;

        public int Id { get { return _mapping.Id; } }

        public string TraktorCommand { get { return getTraktorCommand(); } }

        public string TraktorCommand2 { get { return getTraktorCommand(); } }

        public string Type { get { return Command.MappingType.ToString(); } }

        public TargetType TargetType { get { return Command.Target; } }

        public string AssignmentExpression { get { return Command.AssignmentOptions[Assignment]; } }

        public MappingTargetDeck Assignment
        {
            get { return Command.Assignment; }
            set { Command.Assignment = value; raisePropertyChanged("Assignment"); raisePropertyChanged("AssignmentExpression"); IsChanged = true; }
        }        

        public string Interaction { get { return String.Format("{0} - {1}", Command.ControlType.ToDescriptionString(), Command.InteractionMode.ToDescriptionString()); } }

        public string ConditionExpression {
            get {
                return Conditions.Name ?? Conditions.ToString();
            }
        }

        public string Condition1 {
            get {
                return Conditions.Name ?? Conditions.ToString2("one");
            }
        }

        public string Condition2 {
            get {
                return Conditions.Name ?? Conditions.ToString2("two");
            }
        }

        public string Comment
        {
            get { return _mapping.Comment; }
            set { _mapping.Comment = value; raisePropertyChanged("Comment"); IsChanged = true; }  //todo: why is comment special?
        }

        public string Comment2
        {
            get { return _mapping.Comment; }
            //set { _mapping.Comment = value; raisePropertyChanged("Comment"); IsChanged = true; }  //todo: why is comment special?
        }


        // fixme: split condition1 and condition2
        public ConditionTuple Conditions { get { return _mapping.Conditions; } }

        public ACommand Command { get { return _mapping.Command; } }

        public AMidiDefinition MidiBinding { get { return _mapping.MidiBinding;} }

        public string Blend
        {
            get
            {
                // TODO: refresh this dynamically 
                if (_mapping.MidiBinding == null)
                    return String.Empty;

                //var _control = _mapping.Command.Control;
                //if (_control.HasMethod("Blend")) {

                if (Command.MappingType == MappingType.Out) {
                    bool blend = _mapping.Blend;
                    if (blend) {
                        return "Y";
                    } else {
                        return "N";
                    }
                } else {
                    return String.Empty;
                }
            }
        }

        public string MappedTo
        {
            get
            {
                if(_mapping.MidiBinding == null)
                {
                    return String.Empty;

                } else
                {
                    String ret = _mapping.MidiBinding.Note;

                    // TODO: refresh this dynamically 
                    bool invert = _mapping.Command.Control.Invert;
                    if (invert)
                    {
                        ret = ret + "_I";
                    }

                    return ret;
                }
            }
        }

        public string MappedTo_OnlyNote
        {
            get
            {
                if (_mapping.MidiBinding == null)
                    return String.Empty;

                String note = _mapping.MidiBinding.Note;
                int len = note.Length;

                int i = note.IndexOf('.');
                if (i == -1)
                {
                    return note;
                }

                String channel = note.Substring(0, i);
                String rest = note.Substring(i + 1);

                // TODO: refresh this dynamically 
                bool invert = _mapping.Command.Control.Invert;
                if (invert)
                {
                    rest = rest + "_I";
                }

                String ret = rest + "." + channel;
                return ret;
            }
        }


        public bool CanOverrideFactoryMap {
            get {
                return _mapping.CanOverrideFactoryMap;
            }
        }

        public bool OverrideFactoryMap
        {
            get { return _mapping.OverrideFactoryMap; }
            set { _mapping.OverrideFactoryMap = value; raisePropertyChanged("OverrideFactoryMap"); IsChanged = true; }
        }

        private MappingMetadata _metadata;
        public MappingMetadata Metadata
        {
            get { return _metadata; }
            set { _metadata = value; raisePropertyChanged("Metadata"); }
        }


        public MappingViewModel(Device device, Mapping mapping)
        {
            _device = device;
            _mapping = mapping;
        }


        public void SetBinding(AMidiDefinition binding)
        {
            var changed = _mapping.SetBinding(_device, binding);
            if (changed)
            {
                raisePropertyChanged("MidiBinding");
                raisePropertyChanged("MappedTo");
                raisePropertyChanged("MappedTo_OnlyNote");
                IsChanged = true;
            }
        }

        public void SetCondition(ConditionNumber number, ConditionProxy proxy)
        {
            var changed = _mapping.SetCondition(number, proxy);
            if (changed)
            {
                if (number == ConditionNumber.One)
                    raisePropertyChanged("Condition1");
                else
                    raisePropertyChanged("Condition2");
                raisePropertyChanged("ConditionExpression");
                IsChanged = true;
            }
        }

        public void SetCondition(ConditionNumber number, ACondition condition)
        {
            var changed = _mapping.SetCondition(number, condition);
            if (changed)
            {
                if (number == ConditionNumber.One)
                    raisePropertyChanged("Condition1");
                else
                    raisePropertyChanged("Condition2");
                raisePropertyChanged("ConditionExpression");
                IsChanged = true;
            }
        }

        public Mapping Copy(bool includeMidiBinding)
        {
            return _mapping.Copy(includeMidiBinding);
        }

        public void hack_encoder_mididefinition()
        {
            AGenericMidiDefinition midiBinding = (this._mapping.MidiBinding as AGenericMidiDefinition);
            if(midiBinding == null)
            {
                return;    //nothing todo. We are not linked to a midi modifier yet
            }

            MidiEncoderMode actual_encoder_mode = _mapping.Command.get_EncoderMode();
            midiBinding.MidiEncoderMode = actual_encoder_mode;
        }


        public void UpdateInteraction()
        {
            hack_encoder_mididefinition();

            raisePropertyChanged("Interaction");
            raisePropertyChanged("TraktorCommand");
            IsChanged = true;
        }

        public void UpdateConditionExpression()
        {
            raisePropertyChanged("ConditionExpression");
        }

        public void ChangeAssignment(MappingTargetDeck assignment)
        {
            if (Conditions.Condition1 != null && Conditions.Condition1.Target == TargetType && Conditions.Condition1.Assignment == Assignment)
            {
                Conditions.Condition1.Assignment = assignment;
                raisePropertyChanged("Conditions");
                raisePropertyChanged("ConditionExpression");
            }

            if (Conditions.Condition2 != null && Conditions.Condition2.Target == TargetType && Conditions.Condition2.Assignment == Assignment)
            {
                Conditions.Condition2.Assignment = assignment;
                raisePropertyChanged("Conditions");
                raisePropertyChanged("ConditionExpression");
            }
        
            Assignment = assignment;
        }

        private string getTraktorCommand(bool details = true)
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append(Command.Name);

            if (!details) {
                return sb.ToString();
            }

            switch (_mapping.Command.InteractionMode)
            {
                case MappingInteractionMode.Trigger:
                    break;
                case MappingInteractionMode.Toggle:
                    sb.Append(" [Toggle]");
                    break;
                case MappingInteractionMode.Hold:
                    sb.Append(" [Hold]");
                    break;

                case MappingInteractionMode.Direct:
                    break;
                case MappingInteractionMode.Relative:
                    break;
                case MappingInteractionMode.Increment:
                    sb.Append(" ++");
                    break;
                case MappingInteractionMode.Decrement:
                    sb.Append(" --");
                    break;
                case MappingInteractionMode.Reset:
                    sb.Append(" [Reset]");
                    break;
                case MappingInteractionMode.Output:
                    if (_mapping.Command is EffectSelectorOutCommand)
                    {
                        var efCo = _mapping.Command as EffectSelectorOutCommand;
                        sb.AppendFormat(" [{0}]", efCo.AllEffects ? "All Effects" : efCo.ControllerRangeMin.ToDescriptionString());
                        break;
                    }

                    object min = _mapping.Command.GetType().GetProperty("ControllerRangeMin").GetValue(_mapping.Command);
                    object max = _mapping.Command.GetType().GetProperty("ControllerRangeMax").GetValue(_mapping.Command);
                    if (min.GetType().IsEnum)
                    {
                        min = ((Enum)min).ToDescriptionString();
                        max = ((Enum)max).ToDescriptionString();
                    }
                    else if (min.GetType().Equals(typeof(float)))
                    {
                        min = String.Format("{0:G" + VALUE_TEXT_MAX_PRECISION + "}", min);
                        max = String.Format("{0:G" + VALUE_TEXT_MAX_PRECISION + "}", max);
                    }
                    sb.AppendFormat(" [{0}; {1}]", min, max);
                    break;
                default:
                    break;
            }

            if (_mapping.Command.HasValueUI)
            {
                var pi = _mapping.Command.GetType().GetProperty("Value");
                if (pi != null)
                {
                    object value = pi.GetValue(_mapping.Command);
                    if (value.GetType().IsEnum)
                        value = ((Enum)value).ToDescriptionString();
                    sb.AppendFormat(" = " + "{0:G" + VALUE_TEXT_MAX_PRECISION + "}", value);
                }
            }

            return sb.ToString();
        }


        protected override void Accept()
        {

        }

        protected override void Revert()
        {

        }

        public void hack_modifier(KnownCommands new_id)
        {
            var b = this._mapping;

            b.hack_modifier(new_id);
        }

        public List<string> get_csv_strings(int device_num, bool header = false)
        {
            List<string> ret = new List<string>();

            // FIXME: generate union of condition1 and 2
            if (header) {

                // FIXME: how to write this in C# better ???
                var tmp = new List<string>()
                {
                    "Device",
                    "Id",
                    "Type",
                    "TraktorCommand",
                    "AssignmentExpression",
                    "ConditionExpression",
                    "Interaction",
                    "MappedTo",
                    "Comment",
                };

                ret = tmp;
            } else {
                var tmp = new List<string>()
                {
                    device_num.ToString(),
                    Id.ToString(),
                    Type,
                    TraktorCommand,
                    AssignmentExpression,
                    ConditionExpression,
                    Interaction,
                    MappedTo,
                    Comment,
                };
                ret = tmp;
            };

            return ret;
        }

        public string get_csv_row(string sep, int device, bool header = false)
        {
            var l = get_csv_strings(device, header);
            var st = String.Join(sep, l);
            return st;
        }


        public override bool Equals(object obj)
        {
            MappingViewModel other = obj as MappingViewModel;
            if (other == null)
                return false;
            return _mapping.Equals(other._mapping);
        }

        public override int GetHashCode()
        {
            return _mapping.GetHashCode();
        }
    }
}
