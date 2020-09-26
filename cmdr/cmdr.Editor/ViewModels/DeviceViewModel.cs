using ChangeTracking;
using cmdr.Editor.Metadata;
using cmdr.Editor.Utils;
using cmdr.Editor.ViewModels.Reports;
using cmdr.Editor.AppSettings;
using cmdr.Editor.Utils;
using cmdr.MidiLib;
using cmdr.TsiLib;
using cmdr.TsiLib.Commands;
using cmdr.TsiLib.Enums;
using cmdr.TsiLib.MidiDefinitions.Base;
using cmdr.WpfControls.Behaviors;
using cmdr.WpfControls.DropDownButton;
using cmdr.WpfControls.Utils;
using cmdr.WpfControls.ViewModels;
using cmdr.WpfControls.CustomDataGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Globalization;

namespace cmdr.Editor.ViewModels
{

    public class DeviceViewModel : AReversible
    {
        private TsiFileViewModel _parentSelector;
        public TsiFileViewModel ParentSelector
        {
            get
            {
                return _parentSelector;
            }
            set
            {
                _parentSelector = value;
            }
        }

        public static readonly string ALL_PORTS = "All Ports";
        public static readonly string DEFAULT_PORT = "None";

        private static readonly List<MappingViewModel> _mappingClipboard = new List<MappingViewModel>();
        private static readonly MenuItemViewModel _separator = MenuItemViewModel.Separator;

        private Device _device;
        private string _traktorVersion;

        public int Id { get { return _device.Id; } }

        public string Name { get { return String.IsNullOrWhiteSpace(Comment) ? Type : String.Format("{0} [{1}]", Comment, Type); } }

        public string Type { get { return _device.TypeStr; } }

        public bool IsGenericMidi { get { return Type.Equals(Device.TYPE_STRING_GENERIC_MIDI); } }

        public bool IsKeyboard { get { return _device.IsKeyboard; } }

        public int Revision { get { return _device.Revision; } }

        public string Comment
        {
            get { return _device.Comment; }
            set { _device.Comment = value; raisePropertyChanged("Comment"); raisePropertyChanged("Name"); IsChanged = true; }
        }

        #region Ports

        public List<string> InPorts { get; private set; }

        public string InPort
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_device.InPort))
                    return ALL_PORTS;
                return _device.InPort;
            }
            set
            {
                if (value == ALL_PORTS)
                    _device.InPort = String.Empty;
                else
                    _device.InPort = value; 
                raisePropertyChanged("InPort"); 
                IsChanged = true;
            }
        }

        public List<string> OutPorts { get; private set; }

        public string OutPort
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_device.OutPort))
                    return ALL_PORTS;
                return _device.OutPort;
            }
            set
            {
                if (value == ALL_PORTS)
                    _device.OutPort = String.Empty;
                else
                    _device.OutPort = value;
                raisePropertyChanged("OutPort");
                IsChanged = true;
            }
        }

        #endregion

        public static IEnumerable<DeviceTarget> Targets { get { return Enum.GetValues(typeof(DeviceTarget)).Cast<DeviceTarget>(); } }

        public DeviceTarget Target
        {
            get { return _device.Target; }
            set { _device.Target = value; raisePropertyChanged("Target"); IsChanged = true; }
        }

        private static Dictionary<MidiEncoderMode, string> _encoderModes;
        public static IReadOnlyDictionary<MidiEncoderMode, string> EncoderModes
        {
            get
            {
                if (_encoderModes == null)
                    _encoderModes = Enum.GetValues(typeof(MidiEncoderMode))
                        .Cast<MidiEncoderMode>()
                        .ToDictionary(d => d, d => d.ToDescriptionString());
                return _encoderModes;
            }
        }
        /// <summary>
        /// Encoder mode, specific to a controller and uniform for all of its encoders. Either 3Fh/41h or 7Fh/01h. Only used for generic midi devices.
        /// </summary>
        public MidiEncoderMode EncoderMode
        {
            get { return _device.EncoderMode; }
            set { _device.EncoderMode = value; raisePropertyChanged("EncoderMode"); IsChanged = true; }
        }

        private ObservableCollection<RowItemViewModel> _mappings = new ObservableCollection<RowItemViewModel>();
        public ObservableCollection<RowItemViewModel> Mappings
        {
            get { return _mappings; }
        }

        public IReadOnlyDictionary<string, AMidiDefinition> MidiInDefinitions { get { return _device.MidiInDefinitions; } }
        public IReadOnlyDictionary<string, AMidiDefinition> MidiOutDefinitions { get { return _device.MidiOutDefinitions; } }

        private ObservableCollection<RowItemViewModel> _selectedMappings = new ObservableCollection<RowItemViewModel>();
        public ObservableCollection<RowItemViewModel> SelectedMappings
        {
            get {
                return _selectedMappings;
            }
            set {
                // this is never called. Its the methods that change the collection
                _selectedMappings = value;
                //raisePropertyChanged("TBD");
            }
        }
        
        private MappingEditorViewModel _mappingEditorViewModel;
        public MappingEditorViewModel MappingEditorViewModel
        {
            get { return _mappingEditorViewModel ?? (_mappingEditorViewModel = new MappingEditorViewModel(this, _selectedMappings.Select(r => r.Item as MappingViewModel))); }
            set { _mappingEditorViewModel = value; raisePropertyChanged("MappingEditorViewModel"); }
        }

        private SearchViewModel _searchViewModel;
        public SearchViewModel SearchViewModel
        {
            get { return _searchViewModel ?? (_searchViewModel = new SearchViewModel(this)); }
            set { _searchViewModel = value; raisePropertyChanged("SearchViewModel"); }
        }

        private FilterCmdViewModel _filterCmdViewModel;
        public FilterCmdViewModel FilterCmdViewModel
        {
            get { return _filterCmdViewModel ?? (_filterCmdViewModel = new FilterCmdViewModel(this)); }
            set { _filterCmdViewModel = value; raisePropertyChanged("FilterCmdViewModel"); }
        }


        public ObservableCollection<MenuItemViewModel> InCommands { get; private set; }
        public ObservableCollection<MenuItemViewModel> OutCommands { get; private set; }

        private int InCommands_dynamic_index;
        private int OutCommands_dynamic_index;

        private bool In_has_shortcut;
        private bool Out_has_shortcut;


        public ObservableCollection<MenuItemViewModel> InCommands_wholelist { get; private set; }
        public ObservableCollection<MenuItemViewModel> OutCommands_wholelist { get; private set; }

        public Dictionary<string, AMidiDefinition> DefaultMidiInDefinitions { get; private set; }
        public Dictionary<string, AMidiDefinition> DefaultMidiOutDefinitions { get; private set; }

        private Editor.Metadata.Metadata _metadata;
        public Editor.Metadata.Metadata Metadata
        {
            get { return _metadata; }
            private set { _metadata = value; raisePropertyChanged("Metadata"); }
        }


        #region Commands

        private ICommand _copyCommand;
        public ICommand CopyCommand
        {
            get { return _copyCommand ?? (_copyCommand = new CommandHandler(copy, () => _selectedMappings.Any())); }
        }

        private ICommand _duplicateCommand;
        public ICommand DuplicateCommand
        {
            get { return _duplicateCommand ?? (_duplicateCommand = new CommandHandler(duplicate, () => _selectedMappings.Any())); }
        }

        private ICommand _cutCommand;
        public ICommand CutCommand
        {
            get { return _cutCommand ?? (_cutCommand = new CommandHandler(cut, () => _selectedMappings.Any())); }
        }

        private ICommand _pasteCommand;
        public ICommand PasteCommand
        {
            get { return _pasteCommand ?? (_pasteCommand = new CommandHandler(paste, () => _mappingClipboard != null && _mappingClipboard.Any())); }
        }

        private ICommand _addMappingCommand;
        public ICommand AddMappingCommand
        {
            get { return _addMappingCommand ?? (_addMappingCommand = new CommandHandler<MenuItemViewModel>(addMapping)); }
        }

        private ShowColumns showColumns;

        public bool SplitConditions
        {
            get {  return showColumns.SplitConditions;  }
            set {
                //SetProperty("SplitConditions", ref _splitConditions, ref value);
                showColumns.SplitConditions = value;
                updateShowColumns();
            }
        }

        public bool ShowCommand2
        {
            get {
                return showColumns.ShowCommand2;
            }
            set {
                //SetProperty("SplitConditions", ref _splitConditions, ref value);
                showColumns.ShowCommand2 = value;
                updateShowColumns();
            }
        }

        public bool ShowComment2
        {
            get {
                return showColumns.ShowComment2;
            }
            set {
                //SetProperty("SplitConditions", ref _splitConditions, ref value);
                showColumns.ShowComment2 = value;
                updateShowColumns();
            }
        }

        public bool ShowBlend
        {
            get {
                return showColumns.ShowBlend;
            }
            set {
                //SetProperty("SplitConditions", ref _splitConditions, ref value);
                showColumns.ShowBlend = value;
                updateShowColumns();
            }
        }
        
        public bool HideInteraction
        {
            get {
                return showColumns.HideInteraction;
            }
            set {
                //SetProperty("SplitConditions", ref _splitConditions, ref value);
                showColumns.HideInteraction = value;
                updateShowColumns();
            }
        }

        private bool _showNote;
        public bool ShowNote
        {
            get {
                return showColumns.ShowNote;
            }
            set {
                //SetProperty("SplitConditions", ref _splitConditions, ref value);
                showColumns.ShowNote = value;
                updateShowColumns();
            }
        }


        private ICommand _removeMappingCommand;
        public ICommand RemoveMappingCommand
        {
            get
            {
                return _removeMappingCommand ??
                    (_removeMappingCommand = new CommandHandler(() => removeMappings(_selectedMappings), _selectedMappings.Any));
            }
        }
                      
        private ICommand _showConditionDescriptionsEditorCommand;
        public ICommand ShowConditionDescriptionsEditorCommand
        {
            get { return _showConditionDescriptionsEditorCommand ?? (_showConditionDescriptionsEditorCommand = new CommandHandler(showConditionDescriptionsEditor)); }
        }

        private ICommand _showCommandsReportEditorCommand;
        public ICommand ShowCommandsReportEditorCommand
        {
            get { return _showCommandsReportEditorCommand ?? (_showCommandsReportEditorCommand = new CommandHandler(showCommandsReportEditor)); }
        }

        private ICommand _showMappingsReportCommand;
        public ICommand ShowMappingsReportCommand
        {
            get { return _showMappingsReportCommand ?? (_showMappingsReportCommand = new CommandHandler(showMappingsReportCommand)); }
        }

        private ICommand _refreshPortsCommand;
        public ICommand RefreshPortsCommand
        {
            get { return _refreshPortsCommand ?? (_refreshPortsCommand = new CommandHandler(refreshPorts)); }
        }

        private ICommand _dropCommand;
        public ICommand DropCommand
        {
            get { return _dropCommand ?? (_dropCommand = new CommandHandler<IDataObject>(drop)); }
        }

        private ICommand _selectionChangedCommand;
        public ICommand SelectionChangedCommand
        {
            get { return _selectionChangedCommand ?? (_selectionChangedCommand = new CommandHandler<IList>(updateEditor)); }
        }

        #endregion

        /*
        * search and replace across all selected comments
        */
        public void SedCommentsCommand()
        {
             MappingEditorViewModel.CommentEditor.sedCommentsCommand();

        }


        /*
        * this command duplicates a Selection for all 4 channels:
        * 
        * input:
        *   CMD1 -> ch01.Note.C0 -> deck A
        * 
        * generated:
        *   CMD2 -> ch02.Note.C0 -> deck B
        *   CMD3 -> ch03.Note.C0 -> deck C
        *   CMD4 -> ch04.Note.C0 -> deck D
        */
        public void QuadruplicateCommand()
        {
            for (int i = 1; i < 4; i++) {
                // todo: check if commands are chanell1/DeckA ?

                // Filter on: 
                //   we manipulate the current selection over and over. Thats why we use step 1.
                //   end result is decreasing order
                //
                // Filter off: 
                //   we manipulate the copies
                //   end result is increasing order
                duplicate();
                MappingEditorViewModel.MidiBindingEditor.IncDecChannel(1);
                MappingEditorViewModel.rotateAssignment(1);

                MappingEditorViewModel.CommentEditor.sedCommentsReplaceDeck(i);

            }
        }

        // This command makes 8 copies of the selected entries. 
        // Useful to generate mappings
        public void OctuplicateCommand()
        {
            for(int i=2; i <= 8; i++) {
                // todo: check if commands are chanell1/DeckA ?
                duplicate();
                MappingEditorViewModel.MidiBindingEditor.IncDecNumber(1);
            }
        }


public void BringIntoView(int what)
{
    var selected = this.SelectedMappings;

    if (selected.Any())
    {

        if (what == 0) {
            selected.First().BringIntoView();
        } else
        {
            selected.Last().BringIntoView();
        }
    }

}

// Constructor
public DeviceViewModel(Device device, TsiFileViewModel parent)
{
    // link to parent, used for the filtering operations
    this.ParentSelector = parent;

    _device = device;
    _traktorVersion = _device.TraktorVersion;

    updatePorts(device);

    generateAddMappingContextMenus();

    loadDefaultMidiDefinitionsAsync();

    foreach (var mapping in _device.Mappings)
    {
        var mvm = new MappingViewModel(_device, mapping);
        Mappings.Add(new RowItemViewModel(mvm));
        mvm.DirtyStateChanged += (s, e) => updateMapsChanged();
    }

    loadMetadata();

    AcceptChanges();

    Mappings.CollectionChanged += Mappings_CollectionChanged;

    // set selection if possible
    if (Mappings.Any())
    {
        SelectedMappings.Add(Mappings.First());
        updateAddMappingContextMenus();

        this.ParentSelector.ClearFiltering();
    }
}


// only allow messing with the selection if nothing is filtered. There is too many corner cases
private bool can_change_selection_automatically()
{
    bool has_filtering = this.ParentSelector.HasFiltering();
    bool ret = !has_filtering;
    //ret = false;

    return ret; 
}

public void ClearFiltering()
{
    this.ParentSelector.ClearFiltering();
}

public void ReApplyFiltering()
{
    this.ParentSelector.ReApplyFiltering();
}

        
public void updateShowColumns()
{
    this.ParentSelector.updateShowColumns(showColumns);
}

public void SaveMetadata()
{
    Metadata.DeviceMetadata.MappingMetadata.Clear();
    Metadata.DeviceMetadata.ConditionDescriptions.Clear();
    foreach (var mapping in Mappings.Select(m => m.Item as MappingViewModel))
    {
        if (mapping.Metadata != null)
            Metadata.DeviceMetadata.MappingMetadata[mapping.Id] = mapping.Metadata;

        if (!String.IsNullOrWhiteSpace(mapping.Conditions.Name))
            Metadata.DeviceMetadata.ConditionDescriptions[mapping.Conditions.ToString()] = mapping.Conditions.Name;
    }

    _device.TraktorVersion = _traktorVersion + "|" + cmdr.Editor.Metadata.JsonParser.ToJson(Metadata);
}

public Device Copy(bool includeMappings)
{
    var copy = _device.Copy(includeMappings);
    copy.Comment = "";
    return copy;
}

protected override void Accept()
{
    if (IsChanged)
    {
        _device.IncrementRevision();
        raisePropertyChanged("Revision");
    }

    foreach (var mapping in Mappings)
        (mapping.Item as MappingViewModel).AcceptChanges();
}

protected override void Revert()
{
    foreach (var mapping in Mappings)
        (mapping.Item as MappingViewModel).RevertChanges();
}


private void loadMetadata()
{
    if (_traktorVersion.Contains("|"))
    {
        var parts = _device.TraktorVersion.Split(new[] { '|' }, 2);

        _traktorVersion = parts[0];

        try
        {
            Metadata = cmdr.Editor.Metadata.JsonParser.FromJson(parts[1]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    if (Metadata != null)
    {
        var mappings = Mappings.Select(m => m.Item as MappingViewModel);
        var conditionTuples = mappings.Select(m => m.Conditions);
        string key;
        foreach (var ct in conditionTuples)
        {
            key = ct.ToString();
            if (Metadata.DeviceMetadata.ConditionDescriptions.ContainsKey(key))
                ct.Name = Metadata.DeviceMetadata.ConditionDescriptions[key];
        }

        foreach (KeyValuePair<int, MappingMetadata> mm in Metadata.DeviceMetadata.MappingMetadata)
        {
            var mapping = mappings.FirstOrDefault(m => m.Id == mm.Key);
            if (mapping != null)
                mapping.Metadata = mm.Value;
        }
    }
    else
        Metadata = new Editor.Metadata.Metadata();
}

private void drop(IDataObject dataObject)
{
    if (dataObject == null)
        return;

    var data = dataObject.GetData(typeof(DraggableRowsBehavior.Data)) as DraggableRowsBehavior.Data;
    if (data == null)
        return;

    if (data.TargetIndex < 0 && Mappings.Any()) // // don't allow invalid targets, but allow drop on an empty grid 
        return;

    DeviceViewModel srcDevice = data.SenderDataContext as DeviceViewModel;
    if (srcDevice == null)
        return;

    // copy and sort selected items
    List<RowItemViewModel> selected = new List<RowItemViewModel>(
        data.SelectedItems
        .Cast<RowItemViewModel>()
        .OrderBy(s => srcDevice._mappings.IndexOf(s))
        );

    int newIndex = Math.Max(0, data.TargetIndex);

    if (srcDevice != this || !data.IsMove)
    {
        if (data.IsMove)
            srcDevice.removeMappings(selected);

        SelectedMappings.Clear();

        foreach (var row in selected)
        {
            var rawMapping = (row.Item as MappingViewModel).Copy(true);
            insertMapping(newIndex++, rawMapping);
        }
    }
    else
    {
        var movingAction = new Action<int, int>((oi, ni) =>
            {
                _device.MoveMapping(oi, ni);
                _mappings.Move(oi, ni);
            });
        MovingLogicHelper.Move<RowItemViewModel>(_mappings, selected, newIndex, movingAction);
    }

    if (selected.Any())
        selected.Last().BringIntoView();
}

private void updateMapsChanged()
{
    IsChanged = Mappings.Any(m => (m.Item as MappingViewModel).IsChanged);
}

private void showConditionDescriptionsEditor()
{
    new Views.ConditionDescriptionsEditor
    {
        DataContext = new ConditionTuplesEditorViewModel(Mappings.Select(r => r.Item as MappingViewModel))
    }
    .ShowDialog();
}

private void showCommandsReportEditor()
{

}

private void showMappingsReportCommand()
{

}



// this action is "delete mappings". it is not "delete a device"
private void delete()
{
    // is this function already present?
    removeMappings(SelectedMappings);
}

        private void OnModification()
        {
            var all_count = Mappings.Count;
            var selected_count = SelectedMappings.Count;

            
            if (CmdrSettings.Instance.ClearFilterAtModifications) {
                ClearFiltering();
            } else {
                // ReApplyFiltering(); // is this needed??
            }

            var all_count2 = Mappings.Count;
            var selected_count2 = SelectedMappings.Count;

            // pestrela: be extra carefull
            updateEditor(null);  // is this needed?
        }

private void duplicate()
{
    copy();
    paste();
}

private void cut()
{
    copy();
    removeMappings(SelectedMappings);
}

private void copy()
{
    var sorted = _selectedMappings.OrderBy(s => _mappings.IndexOf(s));

    _mappingClipboard.Clear();
    _mappingClipboard.AddRange(sorted.Select(mvm => mvm.Item as MappingViewModel));
}

private void paste()
{
    var sorted = _selectedMappings.OrderBy(s => _mappings.IndexOf(s));

    int index = _mappings.Count;
    if (_selectedMappings.Count > 0)
    {
        index = _mappings.IndexOf(sorted.Last()) + 1;
        if (can_change_selection_automatically()) {
            SelectedMappings.Clear();
        }
    }

    foreach (var mapping in _mappingClipboard) {
        var new_map = mapping.Copy(true);
        insertMapping(index++, new_map);
    }

    if (can_change_selection_automatically()) {
        _selectedMappings.Last().BringIntoView();
    }

    var all_mappings = _mappings.Count;
    var all_selected_mappings = _selectedMappings.Count;

    OnModification();
}


// This function generates the initial menu tree for the IN and OUT buttons
// Later functions will dynamically add:
//   a) the OUT shortcut
//   b) the filter list
private void generateAddMappingContextMenus()
{
    var builder = new MenuBuilder<CommandProxy>();
    var itemBuilder = new Func<CommandProxy, MenuItemViewModel>(p => new MenuItemViewModel
        {
            Text = p.Name,
            Tag = p
        });

    var allIn = All.KnownInCommands.Select(kv => kv.Value);
    InCommands = new ObservableCollection<MenuItemViewModel>(builder.BuildTree(allIn, itemBuilder, a => a.Category.ToDescriptionString(), "->", false, false));

    var allOut = All.KnownOutCommands.Select(kv => kv.Value);
    OutCommands = new ObservableCollection<MenuItemViewModel>(builder.BuildTree(allOut, itemBuilder, a => a.Category.ToDescriptionString(), "->", false, false));

    // list versions for searching later
    InCommands_wholelist = new ObservableCollection<MenuItemViewModel>(builder.BuildList(allIn, itemBuilder));
    OutCommands_wholelist = new ObservableCollection<MenuItemViewModel>(builder.BuildList(allOut, itemBuilder));


    InCommands.Add(_separator);
    InCommands.Add(_separator);
    OutCommands.Add(_separator);
    OutCommands.Add(_separator);

    // location of the dynamic components of the menu
    InCommands_dynamic_index = InCommands.Count() - 2;
    OutCommands_dynamic_index = InCommands.Count() - 2;

    In_has_shortcut = false;
    Out_has_shortcut = false;

}


void limit_add_mapping_menus_inner(
    string SearchText,
    ObservableCollection<MenuItemViewModel> Commands,
    int seperator_index,
    bool has_shortcut,
    ObservableCollection<MenuItemViewModel> Commands_wholelist)
{
    /// step 1: clean up shortcuts
    if (has_shortcut)
    {
        seperator_index += 1;
    }

    // remove everything after the separator
    while (Commands.Count > seperator_index + 2)
    {
        Commands.RemoveAt(Commands.Count - 1);
    }

    if (string.IsNullOrEmpty(SearchText))
    {
        return;
    }

    /// step 2: generate shortcuts
    var comparer = CultureInfo.CurrentCulture.CompareInfo;
    List<MenuItemViewModel> limited_list;
    limited_list = Commands_wholelist.Where(
            c => comparer.IndexOf(c.Text.ToLower(), SearchText.ToLower(), CompareOptions.IgnoreCase) >= 0
        ).ToList();

    // Remove hundreds of remix deck entries
    List<Categories> blacklist = new List<Categories> {
        Categories.RemixDeck_DirectMapping_Slot1,
        Categories.RemixDeck_DirectMapping_Slot2,
        Categories.RemixDeck_DirectMapping_Slot3,
        Categories.RemixDeck_DirectMapping_Slot4
    };
    foreach(var cat in blacklist)
    {
        limited_list = limited_list.Where(c => ((CommandProxy)(c.Tag)).Category != cat).ToList();
    }

    /// limit the list to the configure value
    int matches = limited_list.Count();
    int max_size = CmdrSettings.Instance.FilterMenuSize;
    limited_list = limited_list.Take(max_size).ToList();

    foreach (var item in limited_list)
    {
        Commands.Add(item);
    }
}


public void limit_add_mapping_menus(string SearchText)
{
    limit_add_mapping_menus_inner(
        SearchText,
        InCommands,
        InCommands_dynamic_index,
        In_has_shortcut,
        InCommands_wholelist);

    limit_add_mapping_menus_inner(
        SearchText,
        OutCommands,
        OutCommands_dynamic_index,
        Out_has_shortcut,
        OutCommands_wholelist);
}

// This function is called everytime we select a new mapping. 
// This is to put the shorthand of the current command in IN/OUT with a seperator 
// This is ONLY for the OUT menu

// FIXME: add IN shortcut as well 
private void updateAddMappingContextMenus()
{

    if (Out_has_shortcut)
    {
        //OutCommands.Remove(_separator);
        OutCommands.RemoveAt(OutCommands_dynamic_index + 1);
    }

    MappingViewModel selectedMapping = null;
    Out_has_shortcut = false;

    if (_selectedMappings.Count != 1 || (selectedMapping = _selectedMappings.Single().Item as MappingViewModel).Command.MappingType != MappingType.In) {
        // Only IN->OUT shortcut is implemented
        return;
    }

    if (All.KnownOutCommands.ContainsKey(selectedMapping.Command.Id))
    {
        var commandProxy = All.KnownOutCommands[selectedMapping.Command.Id];
        if (true)
        {
            Out_has_shortcut = true;

            OutCommands.Insert(OutCommands_dynamic_index + 1, new MenuItemViewModel
            {
                Text = commandProxy.Name + " (" + selectedMapping.AssignmentExpression + ")",
                Tag = commandProxy,
                Command = new CommandHandler<MenuItemViewModel>(i =>
                {
                    int index = _mappings.Count;
                    if (_selectedMappings.Count > 0)
                        index = _mappings.IndexOf(_selectedMappings.Last()) + 1;

                    var proxy = i.Tag as CommandProxy;
                    var m = _device.CreateMapping(proxy);
                    _device.InsertMapping(index, m);

                    var mvm = new MappingViewModel(_device, m);
                    mvm.Assignment = selectedMapping.Assignment;

                    if (selectedMapping.CanOverrideFactoryMap)
                        mvm.OverrideFactoryMap = selectedMapping.OverrideFactoryMap;

                    if (selectedMapping.MidiBinding != null && MidiOutDefinitions.ContainsKey(selectedMapping.MidiBinding.Note))
                        mvm.SetBinding(MidiOutDefinitions[selectedMapping.MidiBinding.Note]);

                    if (selectedMapping.Conditions.Condition1 != null)
                        mvm.SetCondition(TsiLib.Conditions.ConditionNumber.One, selectedMapping.Conditions.Condition1);

                    if (selectedMapping.Conditions.Condition2 != null)
                        mvm.SetCondition(TsiLib.Conditions.ConditionNumber.Two, selectedMapping.Conditions.Condition2);

                    var row = new RowItemViewModel(mvm);
                    _mappings.Insert(index, row);

                    selectExclusive(row);
                    row.BringIntoView();
                })
            });
        }
    }
}

private void insertMapping(int index, Mapping rawMapping)
{
    _device.InsertMapping(index, rawMapping);
    var mvm = new MappingViewModel(_device, rawMapping);
    var row = new RowItemViewModel(mvm);
    _mappings.Insert(index, row);

    if (can_change_selection_automatically()) {
        SelectedMappings.Add(row);
    }

    OnModification();
}

private void addMapping(MenuItemViewModel item)
{
    int index = _mappings.Count;       //add on the last row by default
    if (_selectedMappings.Count > 0)   //if something is selected, add it at the end of the selection
        index = _mappings.IndexOf(_selectedMappings.Last()) + 1;

    var proxy = item.Tag as CommandProxy;

    Mapping m;
    try {
        m = _device.CreateMapping(proxy);
    } catch(Exception e) {
        if (true || CmdrSettings.Instance.VerboseExceptions) {
            MessageBoxHelper.ShowException("Error adding mapping "+item.Text, e);
        }
        return;
    }
    
    _device.InsertMapping(index, m);      // this is to add the mapping itelf

    var mvm = new MappingViewModel(_device, m);
    var row = new RowItemViewModel(mvm);
    _mappings.Insert(index, row);         //this is to add the row to the grid

    selectExclusive(row);
    row.BringIntoView();
}

private void selectExclusive(RowItemViewModel row)
{
    if (can_change_selection_automatically()) {
        SelectedMappings.Clear();
        SelectedMappings.Add(row);
    }
}

private void removeMappings(IEnumerable<RowItemViewModel> mappings)
{
    var selected = new List<RowItemViewModel>(mappings);
    var count = selected.Count();


    // debug purposes
    var real_count = _mappings.Count;
    if(count > real_count) {
        int selected_count = _selectedMappings.Count();

        MessageBoxHelper.ShowFailedAssert(
             string.Format("Error while deleting mappings. Tried to delete {0} mappings, but only {1} exist.", count, real_count));
        return;
    }

    if ((CmdrSettings.Instance.ConfirmDeleteMappingsSize > 0) &&
        (count > CmdrSettings.Instance.ConfirmDeleteMappingsSize)
        ) {
        string msg = String.Format("Are you sure to delete {0} mappings?", count);

        MessageBoxResult messageBoxResult = MessageBox.Show(msg, "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No)
            return;
    }


    foreach (var m in selected)
    {
        _mappings.Remove(m);
        _device.RemoveMapping((m.Item as MappingViewModel).Id);
    }

    OnModification();
}

private void updatePorts(Device device)
{
    IEnumerable<string> inPorts = new[] { DEFAULT_PORT, ALL_PORTS };
    if (!String.IsNullOrWhiteSpace(device.InPort) && !inPorts.Contains(device.InPort))
        inPorts = inPorts.Union(new[] { device.InPort });
    inPorts = inPorts.Union(MidiManager.InputDevices.Select(d => d.Name));
    InPorts = inPorts.ToList();

    IEnumerable<string> outPorts = new[] { DEFAULT_PORT, ALL_PORTS };
    if (!String.IsNullOrWhiteSpace(device.OutPort) && !outPorts.Contains(device.OutPort))
        outPorts = outPorts.Union(new[] { device.OutPort });
    outPorts = outPorts.Union(MidiManager.OutputDevices.Select(d => d.Name));
    OutPorts = outPorts.ToList();
}

private void refreshPorts()
{
    MidiManager.RefreshDevices();
    updatePorts(_device);
    raisePropertyChanged("InPorts");
    raisePropertyChanged("InPort");
    raisePropertyChanged("OutPorts");
    raisePropertyChanged("OutPort");
}

private async void loadDefaultMidiDefinitionsAsync()
{
    if (_device.TypeStr != Device.TYPE_STRING_GENERIC_MIDI)
    {
        var cdm = ControllerDefaultMappings.Instance[_device.TypeStr];
        if (cdm != null)
        {
            if (cdm.DefaultDevice == null)
                await cdm.LoadAsync();

            if (cdm.DefaultDevice != null)
            {
                DefaultMidiInDefinitions = cdm.DefaultDevice.MidiInDefinitions.ToDictionary(d => d.Key, d => d.Value);
                DefaultMidiOutDefinitions = cdm.DefaultDevice.MidiOutDefinitions.ToDictionary(d => d.Key, d => d.Value);
            }
        }
    }
}

private void updateEditor(IList selection)
{
    // do not call on CollectionChanged of SelectedMappings! 
    // otherwise it's called too often because e.g. "select all" builds collection of selected items incrementally
    // should be called when selection is complete and not changing anymore. 
    // therefore it's better to use an EventTrigger on DataGrid's SelectionChanged

    // pestrela: note that "selection" is not used. So we can call this from more places

    var selectedMappingViewModels = _selectedMappings.Select(m => m.Item as MappingViewModel);
    MappingEditorViewModel = new MappingEditorViewModel(this, selectedMappingViewModels);

    updateAddMappingContextMenus();
}

public void selectNone()
{
    if (_selectedMappings.Count > 0)
    {
        SelectedMappings.Clear();
    }
}

public void selectAll()
{
    selectNone();

    foreach (var m in Mappings)
    {
        SelectedMappings.Add(m);
    }
}

public bool is_selected_all()
{
    if (
        (_selectedMappings.Count > 0) &&
        (_selectedMappings.Count == Mappings.Count)
        )
    {
        return true;

    }
    return false;
}

public void selectAllToggle()
{
    if (this.is_selected_all())
        selectNone();
    else
        selectAll();
}

#region Events

void Mappings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
{
    switch (e.Action)
    {
        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
            foreach (RowItemViewModel row in e.NewItems)
                (row.Item as MappingViewModel).DirtyStateChanged += (s, a) => updateMapsChanged();
            break;
        case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
            break;
        case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
            foreach (RowItemViewModel row in e.OldItems)
                (row.Item as MappingViewModel).DirtyStateChanged -= (s, a) => updateMapsChanged();
            break;
        case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
            foreach (RowItemViewModel row in e.OldItems)
                (row.Item as MappingViewModel).DirtyStateChanged -= (s, a) => updateMapsChanged();
            foreach (RowItemViewModel row in e.NewItems)
                (row.Item as MappingViewModel).DirtyStateChanged += (s, a) => updateMapsChanged();
            break;
        case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
            break;
        default:
            break;
    }

    IsChanged = true;
    this.ParentSelector.remember_cgd();  // is this needed?
}

#endregion
}
}
