﻿using System;
using System.Linq;
using System.Security.Policy;
using System.Windows.Forms;
using UpWork.Abstracts;
using UpWork.ConsoleInterface;
using UpWork.Entities;
using UpWork.Enums;

using UpWork.Helpers;
using UpWork.Logger;
using NotificationPublisher = UpWork.NotificationSender.NotificationPublisher;

namespace UpWork.Sides.Employer
{
    public static class EmployerSide
    {
        public static void Start(Entities.Employer employer, Database.Database db)
        {

            
            var employerSideMainLoop = true;

            while (employerSideMainLoop)
            {
                Console.Title = $"Employer: {employer.Name}";

                if (Database.Database.Changes)
                {
                    FileHelper.WriteToJson(db);
                    Database.Database.Changes = false;
                }
                Console.Clear();

                ConsoleScreen.PrintMenu(ConsoleScreen.EmployerSideMainMenu, ConsoleColor.DarkGreen);

                var employerSideChoice =
                    (EmployerSideMainMenu) ConsoleScreen.Input(ConsoleScreen.EmployerSideMainMenu.Count);

                Console.Clear();

                switch (employerSideChoice)
                {
                    case EmployerSideMainMenu.YourAds:
                    {
                        AdsSection.Start(employer);
                        break;
                    }
                    case EmployerSideMainMenu.SeeCvs:
                    {
                        CvSection.Start(employer, db);
                        break;
                    }
                    case EmployerSideMainMenu.AdsNotifications:
                    {
                        while (true)
                        {
                            Console.Clear();
                            if (!ExceptionHandle.Handle(employer.ShowAllAdsWithRequestCount))
                                break;

                            var vacancyId = UserHelper.InputGuid();

                            Vacancy vacancy = null;
                            try
                            {
                                vacancy = employer.GetVacancy(vacancyId);

                                if (vacancy.RequestsFromWorkers.Count != 0)
                                {
                                    var cvs = db.GetAllCvFromRequests(vacancy.RequestsFromWorkers);

                                    while (true)
                                    {
                                        Console.Clear();
                                        if (!ExceptionHandle.Handle(EmployerHelper.ShowCvs, cvs))
                                            break;

                                        var cvId = UserHelper.InputGuid();


                                        var cv = cvs.SingleOrDefault(c => c.Guid == cvId);

                                        if (cv == null)
                                        {
                                            LoggerPublisher.OnLogError($"There is no cv associated this id -> {cvId}");
                                            ConsoleScreen.Clear();
                                            continue;
                                        }

                                        var worker = DatabaseHelper.GetUser(vacancy.RequestsFromWorkers
                                            .SingleOrDefault(r => r.Value == cv.Guid).Key, db.Users) as Worker;


                                        Console.Clear();

                                        Console.WriteLine(cv);

                                        ConsoleScreen.PrintMenu(ConsoleScreen.CvAdsChoices, ConsoleColor.DarkGreen);

                                        var choice =
                                            (CvAdsChoices) ConsoleScreen.Input(ConsoleScreen.CvAdsChoices.Count);


                                        if (choice == CvAdsChoices.Back)
                                            break;


                                        try
                                        {
                                            switch (choice)
                                            {
                                                case CvAdsChoices.Accept:
                                                {
                                                    vacancy.RemoveRequest(worker.Guid);

                                                    NotificationPublisher.OnSend(worker, new Notification(){Title="Your apply result", Message = $"Congratilations. Your cv accepted.\n Vacancy info:\n{vacancy}"});
                                                    LoggerPublisher.OnLogInfo("Accepted.");
                                                    break;
                                                }
                                                case CvAdsChoices.Decline:
                                                {
                                                    vacancy.RemoveRequest(worker.Guid);
                                                    NotificationPublisher.OnSend(worker, new Notification() { Title = "Your apply result", Message = $"We are sorry! Your cv declined.\n Vacancy info:\n{vacancy}" });
                                                    LoggerPublisher.OnLogInfo("Declined.");
                                                    break;
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            LoggerPublisher.OnLogError(e.Message);
                                            ConsoleScreen.Clear();
                                        }

                                        Database.Database.Changes = true;

                                        if (ConsoleScreen.DisplayMessageBox("Info", "Do you want to see other Cvs?",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No) 
                                            break;
                                    }
                                }
                                else
                                {
                                    LoggerPublisher.OnLogError("There is no request!");
                                    ConsoleScreen.Clear();
                                }
                            }
                            catch (Exception e)
                            {
                                LoggerPublisher.OnLogError(e.Message);  
                                ConsoleScreen.Clear();
                            }

                            if (vacancy == null && ConsoleScreen.DisplayMessageBox("Info", "Do you want to try again?",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                                break;
                        }

                        break;
                    }
                    case EmployerSideMainMenu.AllNotifications:
                    {
                        NotificationSide.AllNotificationsStart(employer);
                        break;
                    }
                    case EmployerSideMainMenu.UnreadNotifications:
                    {
                        NotificationSide.OnlyUnReadNotificationsStart(employer);
                        break;
                    }
                    case EmployerSideMainMenu.Logout:
                    {
                        employerSideMainLoop = false;
                        break;
                    }
                }
            }
        }
    }
}