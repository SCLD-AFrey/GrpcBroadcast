using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Windows.Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Google.Protobuf.WellKnownTypes;
using GrpcBroadcast.Client.Core;
using GrpcBroadcast.Common;
using System.Text.Json;
using Newtonsoft.Json;

namespace Example.Client.WpfDxMvvm.ViewModels
{
    [MetadataType(typeof(MetaData))]
    public class MainViewModel
    {
        public class MetaData : IMetadataProvider<MainViewModel>
        {
            void IMetadataProvider<MainViewModel>.BuildMetadata
                (MetadataBuilder<MainViewModel> p_builder)
            {
                p_builder.CommandFromMethod(p_x => p_x.OnLockPersonScriptCommand())
                    .CommandName("LockPersonScriptCommand");
                // p_builder.CommandFromMethod(p_x => p_x.OnUpdatePersonScriptCommand())
                //     .CommandName("UpdatePersonScriptCommand");
                p_builder.Property(p_x => p_x.SelectedPerson)
                    .OnPropertyChangedCall(p_x => p_x.OnSelectedPersonChanged());
            }
        }

        #region Constructors

        protected MainViewModel()
        {
            PersonCollection = new ObservableCollection<Person>();
            BroadcastHistory = new ObservableCollection<string>();
            CommandLine = string.Empty;
            IsReadOnly = false;
            CheckTime = DateTime.UtcNow;

            // ADD PEOPLE
            PersonCollection.Add(new Person(1, "Arthur", "Frey", DateTime.Parse("8/7/1973"), "3043596538", false));
            PersonCollection.Add(new Person(1, "Alyssa", "Frey", DateTime.Parse("3/13/1985"), "3043596539", false));
            PersonCollection.Add(new Person(1, "Fyl", "Frey", DateTime.Parse("10/01/2019"), "", false));
            PersonCollection.Add(new Person(1, "Astrid", "Frey", DateTime.Parse("10/04/2020"), "", false));
            PersonCollection.Add(new Person(1, "Erik", "Frey", DateTime.Parse("10/01/2019"), "", false));
            //-----
            
            
            
            BindingOperations.EnableCollectionSynchronization(BroadcastHistory, m_broadcastHistoryLockObject);
            StartReadingBroadcastServer();

        }

        public static MainViewModel Create()
        {
            return ViewModelSource.Create(() => new MainViewModel());
        }

        #endregion

        #region Fields and Properties

        public virtual ObservableCollection<Person> PersonCollection { get; set; }
        public virtual Person SelectedPerson { get; set; }
        public virtual string CommandLine { get; set; }
        public virtual ObservableCollection<string> BroadcastHistory { get; } = new ObservableCollection<string>();
        public virtual bool IsReadOnly { get; set; }
        public virtual bool IsNotReadOnly { get; set; }
        public virtual DateTime CheckTime { get; set; }

        private readonly object m_broadcastHistoryLockObject = new();
        private static readonly BroadcastServiceClient m_broadcastService = new();
        private static string m_originId = Guid.NewGuid().ToString();

        #endregion

        #region Methods

        public void OnSelectedPersonChanged()
        {
            IsReadOnly = SelectedPerson.IsLocked;
            IsNotReadOnly = !IsReadOnly;
        }
        // public void OnUpdatePersonScriptCommand()
        // {
        //     m_broadcastService.WriteCommandExecute("UPDATE NOT HANDLED YET", m_originId);
        // }
        public void OnLockPersonScriptCommand()
        {
            SelectedPerson.IsLocked = !SelectedPerson.IsLocked;
            m_broadcastService.WriteCommandExecute($"{(SelectedPerson.IsLocked ? "LOCK" : "UNLOCK")} {SelectedPerson.PersonId}", m_originId);
        }


        private void StartReadingBroadcastServer()
        {
            var cts = new CancellationTokenSource();
            _ = m_broadcastService.BroadcastLogs()
                .ForEachAsync(
                    p_x => ProcessCommand(p_x.At, p_x.OriginId, p_x.Content),
                    cts.Token);
        }

        private void ProcessCommand(Timestamp p_at, string p_originId, string p_content)
        {
            CommandLine = $"{p_content} @ {p_at.ToDateTime().ToString("HH:mm:ss")} by {p_originId}" ;
            //BroadcastHistory.Clear();
            if (CheckTime < p_at.ToDateTime() && p_originId != m_originId)
            {

                try
                {
                    var personID = int.Parse(p_content.Split(' ')[1]);
                    var p = PersonCollection.Single(x => x.PersonId == personID);
                        switch (p_content.Split(' ')[0].ToUpper())
                        {
                            case "LOCK":
                                p.IsLocked = true;
                                break;
                            case "UNLOCK":
                                p.IsLocked = false;
                                break;
                        }

                        BroadcastHistory.Add(
                            $"{p_at.ToDateTime().ToString("HH:mm:ss")} {p_originId} : {p_content.Split(' ')[0].ToUpper()}-{p_content.Split(' ')[1]}");


                }
                catch (Exception e)
                {
                    //BroadcastHistory.Add($"{p_at.ToDateTime().ToString("HH:mm:ss")} {p_originId} : {p_content} - {e.Message}");
                }


            }

            CheckTime = DateTime.UtcNow;
        }


        #endregion


    }
}