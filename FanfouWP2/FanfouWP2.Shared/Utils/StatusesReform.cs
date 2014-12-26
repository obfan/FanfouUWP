﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FanfouWP2.FanfouAPI.Items;

namespace FanfouWP2.Utils
{
    public class StatusesReform
    {
        public static void reform(ObservableCollection<Status> statuses, List<Status> list)
        {
            lock (new Object())
            {
                if (list.Count == 0)
                {
                    goto end;
                }
                else if (statuses.Count == 0)
                {
                    foreach (Status item in list)
                        statuses.Add(item);
                    goto end;
                }
                else if (list.Last().rawid > statuses.Last().rawid)
                {
                    list.Reverse();
                    if (list.Count >= 20)
                    {
                        statuses.Insert(0, new Status { is_refresh = true });
                    }

                    foreach (Status i in list)
                    {
                        if ((from s in statuses where s.id == i.id select s).Count() == 0)
                            statuses.Insert(0, i);
                    }
                    goto end;

                }
                else if (list.First().rawid < statuses.First().rawid)
                {
                    foreach (Status i in list)
                    {
                        if ((from s in statuses where s.id == i.id select s).Count() == 0)
                            statuses.Add(i);
                    }
                    goto end;
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        int j = 0;
                        for (j = 0; j < statuses.Count; j++)
                        {
                            if (list[i].rawid < statuses[j].rawid)
                                continue;
                            if (list[i].rawid > statuses[j].rawid)
                                break;
                            if (list[i].rawid == statuses[j].rawid)
                                goto equal;
                        }

                        if ((from s in statuses where s.id == list[i].id select s).Count() == 0)
                            statuses.Insert(j, list[i]);
                    equal:
                        ;
                    }
                    int k = 0;
                    for (k = 0; k < statuses.Count; k++)
                    {
                        if (list.Last().rawid < statuses[k].rawid)
                        {
                            if (list.Count >= 20)
                            {
                                statuses.Insert(k, new Status { is_refresh = true });
                            }
                            break;
                        }
                    }
                    goto end;
                }

            end:
                if (statuses.Count > 0)
                {
                    if (statuses[0].is_refresh == true)
                        statuses.Remove(statuses[0]);
                    if (statuses[statuses.Count - 1].is_refresh == true)
                        statuses.Remove(statuses[statuses.Count - 1]);
                    for (int i = 0; i < statuses.Count - 1; i++)
                    {
                        if (statuses[i].is_refresh == true && statuses[i + 1].is_refresh == true)
                        {
                            statuses.Remove(statuses[i + 1]);
                            i += 1;
                        }
                    }
                }
            }
        }
    }
}