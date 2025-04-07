﻿//Description:
//Records a list of accounts in a domain and the group membership for each account
//Output is stored in a subdirectory of the directory where the program is run by the date the program was run on

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.DirectoryServices;
using Microsoft.Win32;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;

namespace AccountRecorder
{
    class ARMain
    {
        struct CMDArguments
        {
            public bool bEventLogStartStop;
            public bool bParseCmdArguments;
            public string strPrincipalContextType;
        }

        static bool funcLicenseCheck()
        {
            string strLicenseString = "";
            bool bValidLicense = false;

            try
            {
                TextReader tr = new StreamReader("sotfwlic.dat");

                try
                {
                    strLicenseString = tr.ReadLine();

                    if (strLicenseString.Length > 0 & strLicenseString.Length < 29)
                    {
                        // [DebugLine] Console.WriteLine("if: " + strLicenseString);
                        Console.WriteLine("Invalid license");

                        tr.Close(); // close license file

                        return bValidLicense;
                    }
                    else
                    {
                        tr.Close(); // close license file
                        // [DebugLine] Console.WriteLine("else: " + strLicenseString);

                        string strMonthTemp = ""; // to convert the month into the proper number
                        string strDate;

                        //Month
                        strMonthTemp = strLicenseString.Substring(7, 1);
                        if (strMonthTemp == "A")
                        {
                            strMonthTemp = "10";
                        }
                        if (strMonthTemp == "B")
                        {
                            strMonthTemp = "11";
                        }
                        if (strMonthTemp == "C")
                        {
                            strMonthTemp = "12";
                        }
                        strDate = strMonthTemp;

                        //Day
                        strDate = strDate + "/" + strLicenseString.Substring(16, 1);
                        strDate = strDate + strLicenseString.Substring(6, 1);

                        // Year
                        strDate = strDate + "/" + strLicenseString.Substring(24, 1);
                        strDate = strDate + strLicenseString.Substring(4, 1);
                        strDate = strDate + strLicenseString.Substring(1, 2);

                        // [DebugLine] Console.WriteLine(strDate);
                        // [DebugLine] Console.WriteLine(DateTime.Today.ToString());
                        DateTime dtLicenseDate = DateTime.Parse(strDate);
                        // [DebugLine]Console.WriteLine(dtLicenseDate.ToString());

                        if (dtLicenseDate >= DateTime.Today)
                        {
                            bValidLicense = true;
                        }
                        else
                        {
                            Console.WriteLine("License expired.");
                        }

                        return bValidLicense;
                    }

                } //end of try block on tr.ReadLine

                catch
                {
                    // [DebugLine] Console.WriteLine("catch on tr.Readline");
                    Console.WriteLine("Invalid license");
                    tr.Close();
                    return bValidLicense;

                } //end of catch block on tr.ReadLine

            } // end of try block on new StreamReader("sotfwlic.dat")

            catch (System.Exception ex)
            {
                // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());

                // [DebugLine] System.Console.WriteLine(ex.Message);

                if (ex.Message.StartsWith("Could not find file"))
                {
                    Console.WriteLine("License file not found.");
                }

                return bValidLicense;

            } // end of catch block on new StreamReader("sotfwlic.dat")
        }

        static bool funcLicenseActivation()
        {
            try
            {
                if (funcCheckForFile("TurboActivate.dll"))
                {
                    if (funcCheckForFile("TurboActivate.dat"))
                    {
                        TurboActivate.VersionGUID = "4935355894e0da3d4465e86.37472852";

                        if (TurboActivate.IsActivated())
                        {
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("A license for this product has not been activated.");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("TurboActivate.dat is required and could not be found.");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("TurboActivate.dll is required and could not be found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return false;
            }
        }

        static void funcPrintParameterWarning()
        {
            Console.WriteLine("A parameter is missing or is incorrect.");
            Console.WriteLine("Run AccountRecorder -? to get the parameter syntax.");
        }

        static void funcPrintParameterSyntax()
        {
            Console.WriteLine("AccountRecorder v1.0 (c) 2011 SystemsAdminPro.com");
            Console.WriteLine();
            Console.WriteLine("Description: generate list of user accounts in the domain and account group membership");
            Console.WriteLine();
            Console.WriteLine("Parameter syntax:");
            Console.WriteLine();
            Console.WriteLine("Use the following for the first parameter:");
            Console.WriteLine("-run                required parameter");
            Console.WriteLine();
            Console.WriteLine("Use the following for the second parameter:");
            Console.WriteLine("-domain             to use the domain the computer is a member of");
            //Console.WriteLine("-local              to use the local machine");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("AccountRecorder -run -domain");
            //Console.WriteLine("AccountRecorder -run -local");
        }

        static CMDArguments funcParseCmdArguments(string[] cmdargs)
        {
            CMDArguments objCMDArguments = new CMDArguments();

            objCMDArguments.strPrincipalContextType = "";
            objCMDArguments.bEventLogStartStop = false;

            try
            {

                if (cmdargs[0] == "-run" & cmdargs.Length == 2)
                {
                    if (cmdargs[1] == "-domain" | cmdargs[1] == "-local")
                    {
                        if (cmdargs[1] == "-domain")
                        {
                            objCMDArguments.strPrincipalContextType = "Domain";
                            objCMDArguments.bParseCmdArguments = true;
                        }
                        else
                        {
                            objCMDArguments.strPrincipalContextType = "Local";
                            objCMDArguments.bParseCmdArguments = true;
                        }
                    }
                }
                else
                {
                    objCMDArguments.bParseCmdArguments = false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

            // remove next line if necessary; planted to remove initial error condition when creating function
            return objCMDArguments;
        }

        static void funcProgramExecution(CMDArguments objCMDArguments2)
        {
            funcToEventLog("AccountRecorder", "AccountRecorder started", 100);

            funcProgramRegistryTag();

            DateTime dtNow = DateTime.Now;

            string dtFormat = "MMdyyyyHHmmss"; // for log file creation

            string dtFormat2 = "MMdyyyy"; // for log file directory creation

            string strPath = Directory.GetCurrentDirectory();

            if (!Directory.Exists(strPath + "\\Log"))
            {
                Directory.CreateDirectory(strPath + "\\Log");
                if (Directory.Exists(strPath + "\\Log"))
                {
                    strPath = strPath + "\\Log";
                }
            }
            else
            {
                strPath = strPath + "\\Log";
            }

            string strDirPath = strPath + "\\AR" + dtNow.ToString(dtFormat2);

            string strLogFileName = strDirPath + "\\AccountRecorder-" + dtNow.ToString(dtFormat) + dtNow.Millisecond.ToString() + ".log";

            if (!Directory.Exists(strDirPath))
            {
                Directory.CreateDirectory(strDirPath);
            }

            TextWriter twLogFileWriter = new StreamWriter(strLogFileName);

            twLogFileWriter.WriteLine("Name" + "\t" + "SAMAccountName" + "\t" + "Status" + "\t" + "EmailAddress" + "\t" + "SID" +
                                      "\t" + "DN");

            PrincipalContext ctx = funcCreatePrincipalContext(objCMDArguments2.strPrincipalContextType);

            // Create an in-memory user object to use as the query example.
            UserPrincipal u = new UserPrincipal(ctx);

            // Set properties on the user principal object.
            //u.GivenName = "Jim";
            //u.Surname = "Daly";

            // Create a PrincipalSearcher object to perform the search.
            PrincipalSearcher ps = new PrincipalSearcher();

            // Tell the PrincipalSearcher what to search for.
            ps.QueryFilter = u;

            // Run the query. The query locates users 
            // that match the supplied user principal object. 
            PrincipalSearchResult<Principal> results = ps.FindAll();

            string objAccountDEvalues;
            string strAccountStatus = "";

            foreach (UserPrincipal user in results)
            {
                strAccountStatus = "";

                if (user.Enabled == true)
                {
                    strAccountStatus = "Enabled";
                }
                else
                {
                    strAccountStatus = "Disabled";
                }

                //Console.WriteLine("{0} \t {1} \t {2} \t {3} \t {4} \t {5}", user.Name, user.SamAccountName, strAccountStatus, user.EmailAddress
                //    , user.Sid, user.DistinguishedName);

                objAccountDEvalues = user.Name + "\t" + user.SamAccountName + "\t" + strAccountStatus + "\t" + user.EmailAddress
                    + "\t" + user.Sid + "\t" + user.DistinguishedName;

                twLogFileWriter.WriteLine(objAccountDEvalues);

                funcGetUserGroups(user, strLogFileName);
            }

            twLogFileWriter.Close();

            funcToEventLog("AccountRecorder", "AccountRecorder stopped", 101);

        }

        static void funcProgramRegistryTag()
        {
            try
            {
                string strRegistryProfilesPath = "SOFTWARE";
                RegistryKey objRootKey = Microsoft.Win32.Registry.LocalMachine;
                RegistryKey objSoftwareKey = objRootKey.OpenSubKey(strRegistryProfilesPath, true);
                RegistryKey objSystemsAdminProKey = objSoftwareKey.OpenSubKey("SystemsAdminPro", true);
                if (objSystemsAdminProKey == null)
                {
                    objSystemsAdminProKey = objSoftwareKey.CreateSubKey("SystemsAdminPro");
                }
                if (objSystemsAdminProKey != null)
                {
                    if (objSystemsAdminProKey.GetValue("AccountRecorder") == null)
                        objSystemsAdminProKey.SetValue("AccountRecorder", "1", RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static void funcToEventLog(string strAppName, string strEventMsg, int intEventType)
        {
            try
            {
                string strLogName;

                strLogName = "Application";

                if (!EventLog.SourceExists(strAppName))
                    EventLog.CreateEventSource(strAppName, strLogName);

                //EventLog.WriteEntry(strAppName, strEventMsg);
                EventLog.WriteEntry(strAppName, strEventMsg, EventLogEntryType.Information, intEventType);
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static DirectorySearcher funcCreateDSSearcher()
        {
            try
            {
                // [Comment] Get local domain context
                string rootDSE;

                System.DirectoryServices.DirectorySearcher objrootDSESearcher = new System.DirectoryServices.DirectorySearcher();
                rootDSE = objrootDSESearcher.SearchRoot.Path;
                //Console.WriteLine(rootDSE);

                // [Comment] Construct DirectorySearcher object using rootDSE string
                System.DirectoryServices.DirectoryEntry objrootDSEentry = new System.DirectoryServices.DirectoryEntry(rootDSE);
                System.DirectoryServices.DirectorySearcher objDSSearcher = new System.DirectoryServices.DirectorySearcher(objrootDSEentry);
                //Console.WriteLine(objDSSearcher.SearchRoot.Path);
                return objDSSearcher;
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return null;
            }
        }

        static void funcGetUserGroups(UserPrincipal user2, string strUserGroupFileNamePrefix)
        {
            // [DebugLine]Console.WriteLine("funcGetUserGroups called for: {0}", user2.Name);

            string strUserGroupFileName = strUserGroupFileNamePrefix.Substring(0, strUserGroupFileNamePrefix.Length - 4) + "-" + user2.Name + ".log";

            try
            {
                TextWriter twUserGroupFileWriter = new StreamWriter(strUserGroupFileName);

                foreach (Principal p in user2.GetGroups())
                {
                    twUserGroupFileWriter.WriteLine(p.Name + "\t" + p.DistinguishedName);
                }

                twUserGroupFileWriter.Close();
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static PrincipalContext funcCreatePrincipalContext(string strContextType)
        {
            try
            {
                Domain objDomain = Domain.GetComputerDomain();
                string strDomain = objDomain.Name;
                DirectorySearcher tempDS = funcCreateDSSearcher();
                string strDomainRoot = tempDS.SearchRoot.Path.Substring(7);
                // [DebugLine] Console.WriteLine(strDomainRoot);

                if (strContextType == "Domain")
                {

                    PrincipalContext newctx = new PrincipalContext(ContextType.Domain,
                                                    strDomain,
                                                    strDomainRoot);
                    return newctx;
                }
                else
                {
                    PrincipalContext newctx = new PrincipalContext(ContextType.Machine);
                    return newctx;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return null;
            }
        }

        static void funcGetFuncCatchCode(string strFunctionName, Exception currentex)
        {
            string strCatchCode = "";

            Dictionary<string, string> dCatchTable = new Dictionary<string, string>();
            dCatchTable.Add("funcGetFuncCatchCode", "f0");
            dCatchTable.Add("funcLicenseCheck", "f1");
            dCatchTable.Add("funcPrintParameterWarning", "f2");
            dCatchTable.Add("funcPrintParameterSyntax", "f3");
            dCatchTable.Add("funcParseCmdArguments", "f4");
            dCatchTable.Add("funcProgramExecution", "f5");
            dCatchTable.Add("funcProgramRegistryTag", "f6");
            dCatchTable.Add("funcCreateDSSearcher", "f7");
            dCatchTable.Add("funcCreatePrincipalContext", "f8");
            dCatchTable.Add("funcCheckNameExclusion", "f9");
            dCatchTable.Add("funcMoveDisabledAccounts", "f10");
            dCatchTable.Add("funcFindAccountsToDisable", "f11");
            dCatchTable.Add("funcCheckLastLogin", "f12");
            dCatchTable.Add("funcRemoveUserFromGroup", "f13");
            dCatchTable.Add("funcToEventLog", "f14");
            dCatchTable.Add("funcCheckForFile", "f15");
            dCatchTable.Add("funcCheckForOU", "f16");
            dCatchTable.Add("funcWriteToErrorLog", "f17");
            dCatchTable.Add("funcGetUserGroups", "f18");

            if (dCatchTable.ContainsKey(strFunctionName))
            {
                strCatchCode = "err" + dCatchTable[strFunctionName] + ": ";
            }

            //[DebugLine] Console.WriteLine(strCatchCode + currentex.GetType().ToString());
            //[DebugLine] Console.WriteLine(strCatchCode + currentex.Message);

            funcWriteToErrorLog(strCatchCode + currentex.GetType().ToString());
            funcWriteToErrorLog(strCatchCode + currentex.Message);

        }

        static void funcWriteToErrorLog(string strErrorMessage)
        {
            try
            {
                string strPath = Directory.GetCurrentDirectory();

                if (!Directory.Exists(strPath + "\\Log"))
                {
                    Directory.CreateDirectory(strPath + "\\Log");
                    if (Directory.Exists(strPath + "\\Log"))
                    {
                        strPath = strPath + "\\Log";
                    }
                }
                else
                {
                    strPath = strPath + "\\Log";
                }

                FileStream newFileStream = new FileStream(strPath + "\\Err-AccountRecorder.log", FileMode.Append, FileAccess.Write);
                TextWriter twErrorLog = new StreamWriter(newFileStream);

                DateTime dtNow = DateTime.Now;

                string dtFormat = "MMddyyyy HH:mm:ss";

                twErrorLog.WriteLine("{0} \t {1}", dtNow.ToString(dtFormat), strErrorMessage);

                twErrorLog.Close();
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

        }

        static bool funcCheckForFile(string strInputFileName)
        {
            try
            {
                if (System.IO.File.Exists(strInputFileName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return false;
            }
        }

        static void Main(string[] args)
        {
            //if (funcLicenseCheck())
            if (funcLicenseActivation())
            {
                if (args.Length == 0)
                {
                    funcPrintParameterWarning();
                }
                else
                {
                    if (args[0] == "-?")
                    {
                        funcPrintParameterSyntax();
                    }
                    else
                    {
                        string[] arrArgs = args;
                        CMDArguments objArgumentsProcessed = funcParseCmdArguments(arrArgs);

                        if (objArgumentsProcessed.bParseCmdArguments)
                        {
                            funcProgramExecution(objArgumentsProcessed);
                        }
                        else
                        {
                            funcPrintParameterWarning();
                        } // check objArgumentsProcessed.bParseCmdArguments
                    } // check args[0] = "-?"
                } // check args.Length == 0
            } // funcLicenseCheck()

        } // Main()

    } // class ARMain
} // namespace AccountRecorder
