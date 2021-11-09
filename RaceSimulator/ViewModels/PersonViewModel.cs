using Microsoft.Win32;
using RaceSimulator.Core;
using RaceSimulator.Interfaces;
using RaceSimulator.Models;
using SwissTiming.Timing.AcquisitionSimulator;
using SCompetitor = SwissTiming.Timing.AcquisitionSimulator.Competitor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using Competitor = RaceSimulator.Models.Competitor;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Data;

namespace RaceSimulator.ViewModels
{
    public class PersonViewModel : PersonChange
    {

        public PersonViewModel()
        {

        }

        private readonly object _lock = new object();
        private int _rank = 1;

        private ObservableCollection<Person> _personList = new ObservableCollection<Person>();

        public ObservableCollection<Person> PersonList
        {
            get { return _personList; }
            set
            {
                _personList = value;
                OnPropertyChanged("PersonList");
            }
        }

        private ObservableCollection<Competitor> _competitorList = new ObservableCollection<Competitor>();

        public ObservableCollection<Competitor> CompetitorList
        {
            get { return _competitorList; }
            set
            {
                _competitorList = value;
                OnPropertyChanged("CompetitorList");
            }
        }

        private ObservableCollection<FinalResult> _finalResults = new ObservableCollection<FinalResult>();

        public ObservableCollection<FinalResult> FinalResultList
        {
            get { return _finalResults; }
            set
            {
                _finalResults = value;
                OnPropertyChanged("FinalResultList");
            }
        }

        private Person _currentPerson;

        private Competitor _currentCompetitor;

        private FinalResult _currentFinalCompetitor;

        public Person CurrentPerson
        {
            get { return _currentPerson; }
            set
            {
                _currentPerson = value;
                OnPropertyChanged("CurrentPerson");
            }
        }

        public Competitor CurrentCompetitor
        {
            get { return _currentCompetitor; }
            set
            {
                _currentCompetitor = value;
                OnPropertyChanged("CurrentCompetitor");
            }
        }

        public FinalResult CurrentFinalCompetitor
        {
            get { return _currentFinalCompetitor; }
            set { _currentFinalCompetitor = value; }
        }

        private string _selectedPath;

        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                _selectedPath = value;
                OnPropertyChanged("SelectedPath");
            }
        }

        private ICommand _listPersonCommand;

        public ICommand ListPersonCommand
        {
            get
            {
                if (_listPersonCommand == null)
                    _listPersonCommand = new RelayCommand(new Action(LoadPersons));
                return _listPersonCommand;
            }
        }

        private ICommand _addCompetitorCommand;

        public ICommand AddCompetitorCommand
        {
            get
            {
                if (_addCompetitorCommand == null)
                    _addCompetitorCommand = new RelayCommand(new Action(AddCompetitor));
                return _addCompetitorCommand;
            }
        }

        private ICommand _deleteCompetitorCommand;

        public ICommand DeleteCompetitorCommand
        {
            get
            {
                if (_deleteCompetitorCommand == null)
                    _deleteCompetitorCommand = new RelayCommand(new Action(DeleteCompetitor));
                return _deleteCompetitorCommand;
            }
        }

        private ICommand _runCompetition;

        public ICommand RunCompetition
        {
            get
            {
                if (_runCompetition == null)
                    _runCompetition = new RelayCommand(new Action(StartRace));
                return _runCompetition;
            }
        }

        private ICommand _exportResults;

        public ICommand ExportResults
        {
            get
            {
                if (_exportResults == null)
                    _exportResults = new RelayCommand(new Action(ExportResult));
                return _exportResults;
            }
        }

        private void LoadPersons()
        {
            PersonList = LoadXml();
        }

        public ObservableCollection<Person> LoadXml()
        {
            OpenFileDialog fd = new OpenFileDialog();

            if (fd.ShowDialog() == true)
            {
                SelectedPath = fd.FileName;

                var fileStream = fd.OpenFile();

                XmlRootAttribute xRoot = new XmlRootAttribute();
                xRoot.ElementName = "EntryList";
                xRoot.IsNullable = true;

                var xmls = new XmlSerializer(typeof(Person[]), xRoot);

                var persons = (Person[])xmls.Deserialize(fileStream);
                ObservableCollection<Person> personCollection = new ObservableCollection<Person>();

                foreach (var person in persons)
                {
                    personCollection.Add(new Person(person.id, person.country, person.Name));
                }

                personCollection = new ObservableCollection<Person>(personCollection.OrderBy(x => Int32.Parse(x.id)));
                return personCollection;

            }

            return new ObservableCollection<Person>();
        }

        private void AddCompetitor()
        {
            if (CurrentPerson != null)
            {
                var max = CompetitorList.Count > 0 ? CompetitorList.Max(x => x.Bib) : 0;

                CompetitorList.Add(new Competitor(CurrentPerson, max + 1));
                PersonList.Remove(CurrentPerson);
                CompetitorList = new ObservableCollection<Competitor>(CompetitorList.OrderBy(x => Int32.Parse(x.Person.id)));
            }
        }

        private void DeleteCompetitor()
        {
            if (CurrentCompetitor != null)
            {
                PersonList.Add(CurrentCompetitor.Person);
                CompetitorList.Remove(CurrentCompetitor);
                PersonList = new ObservableCollection<Person>(PersonList.OrderBy(x => Int32.Parse(x.id)));
                CompetitorList = new ObservableCollection<Competitor>(CompetitorList.OrderBy(x => Int32.Parse(x.Person.id)));
            }
        }

        private void ExportResult()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML-File | *.xml";
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    var filename = sfd.FileName;

                    XmlSerializer xs = new XmlSerializer(typeof(List<FinalResult>));
                    using (StreamWriter wr = new StreamWriter(File.Create(filename)))
                    {
                        xs.Serialize(wr, FinalResultList.ToList());
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
            }
        }

        private void StartRace()
        {
            //get option from App.comfig
            int startKind = Int32.Parse(ConfigurationManager.AppSettings["StartKind"]);

            Simulator s = new Simulator((StartKind)startKind);
 
            List<SCompetitor> competitors = new List<SCompetitor>();

            //Add Competitors from ViewModel
            foreach (var item in CompetitorList)
            {
                competitors.Add(new SCompetitor(item.Person.Name, item.Bib));
            }

            s.Initialize(competitors);
            //to modify the Collection in others Threads.
            BindingOperations.EnableCollectionSynchronization(FinalResultList, _lock);
            s.RaceStarted += S_RaceStarted;
            s.RaceCompleted += s_RaceCompleted;
            s.CompetitorChanged += S_CompetitorChanged;
            
            s.Start();

            while (s.IsRunning)
            {

            }

            OrderList();
        }

        private void OrderList()
        {
            try
            {
                FinalResultList = new ObservableCollection<FinalResult>(FinalResultList.OrderBy(x => x.Rank));

                var dnf = FinalResultList.Where(x => x.IrmKind == IrmKind.DNF).ToList();
                var dsq = FinalResultList.Where(x => x.IrmKind == IrmKind.DSQ).ToList();
                var dns = FinalResultList.Where(x => x.IrmKind == IrmKind.DNS).ToList();

                foreach (var item in dnf)
                {
                    FinalResultList.Remove(item);
                }

                foreach (var item in dsq)
                {
                    FinalResultList.Remove(item);
                }

                foreach (var item in dns)
                {
                    FinalResultList.Remove(item);
                }

                foreach (var item in dnf)
                {
                    FinalResultList.Add(item);
                }

                foreach (var item in dsq)
                {
                    FinalResultList.Add(item);
                }

                foreach (var item in dns)
                {
                    FinalResultList.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void S_RaceStarted(object sender, RaceStartedEventArgs e)
        {
            var competitor = CompetitorList.FirstOrDefault(x => x.Bib == e.Bib);
            FinalResultList.Add(new FinalResult(competitor, null, null, null, null, e.EventTime, string.Empty));
        }

        private void S_CompetitorChanged(object sender, CompetitorChangedEventArgs e)
        {
            var finalCompetitor = FinalResultList.FirstOrDefault(x => x.Competitor.Bib == e.Bib);
            if (finalCompetitor != null)
            {
                finalCompetitor.IrmKind = e.Irm;
                finalCompetitor.Comment = e.Reason;
            }
        }

        private void s_RaceCompleted(object sender, RaceCompletedEventArgs e)
        {
            var finalCompetitor = FinalResultList.FirstOrDefault(x => x.Competitor.Bib == e.Bib);

            if (finalCompetitor != null)
            {
                finalCompetitor.FinalTime = e.EventTime;
                finalCompetitor.NetTime = Convert.ToDateTime((finalCompetitor.FinalTime - finalCompetitor.StartTime).ToString());
                finalCompetitor.IrmKind = IrmKind.None;
            }

            //Rank
            //Order List

            finalCompetitor.Rank = _rank;

            //FinalResultList = new ObservableCollection<FinalResult>(FinalResultList.OrderBy(x => x.Rank));

            _rank++;
        }
    }
}
