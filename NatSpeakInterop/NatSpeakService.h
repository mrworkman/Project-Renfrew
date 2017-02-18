// Project Renfrew
// Copyright(C) 2016  Stephen Workman (workman.stephen@gmail.com)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//

// NOTE:
//   Portions (c) Copyright 1997-1999 by Joel Gould.

#pragma once

// Native types are private by default with /clr
#pragma make_public(::IServiceProvider)

#include "Stdafx.h"
#include <vcclr.h>

#define IServiceProviderGUID "6d5140c1-7436-11ce-8034-00aa006009fa"
#define IDgnSiteGUID "dd100006-6205-11cf-ae61-0000e8a28647"

#define  E_BUFFERTOOSMALL     0x8004020D
#define  SRERR_NOUSERSELECTED 0x8004041A

namespace Renfrew::NatSpeakInterop {
   using namespace System;
   using namespace System::Diagnostics;
   using namespace System::Runtime::InteropServices::ComTypes;

   using namespace Renfrew::Helpers;
   using namespace Renfrew::NatSpeakInterop::Dragon::ComInterfaces;
   using namespace Renfrew::NatSpeakInterop::Sinks;

   public ref class NatSpeakService {
      private:
         ::IServiceProvider *_piServiceProvider;

         ISrCentral ^_isrCentral = nullptr;
         IDgnSpeechServices  ^_idgnSpeechServices = nullptr;
         IDgnSSvcOutputEvent ^_idgnSSvcOutputEvent = nullptr;
         IDgnSSvcInterpreter ^_idgnSSvcInterpreter = nullptr;

         DWORD key;
         
      private:
         void RegisterEngineSink();
         void RegisterSpeechServiceSinks();
         
      public:
         NatSpeakService();
         ~NatSpeakService();

         void Connect(IntPtr site);
         void Connect(::IServiceProvider *site);
         void Disconnect();

         IntPtr CreateSiteObject();
         void ReleaseSiteObject(IntPtr sitePtr);

         String ^GetCurrentUserProfileName();
         String ^GetUserDirectory(String ^userProfile);

   };

   NatSpeakService::NatSpeakService() {
      key = 0;
   }

   NatSpeakService::~NatSpeakService() { }

   void NatSpeakService::Connect(IntPtr site) {
      Connect(reinterpret_cast<::IServiceProvider*>(site.ToPointer()));
   }

   void NatSpeakService::Connect(::IServiceProvider *site) {
      if (site == nullptr)
         throw gcnew ArgumentNullException();

      _piServiceProvider = site;

      ISrCentral ^*ptr = ComHelper::QueryService<IDgnDictate^, ISrCentral^>(_piServiceProvider);
      _isrCentral = (ISrCentral^) Marshal::GetObjectForIUnknown(IntPtr(ptr));

      RegisterEngineSink();
      RegisterSpeechServiceSinks();
   }

   IntPtr NatSpeakService::CreateSiteObject() {
      IntPtr sitePtr;
      
      Guid iServiceProviderGuid(IServiceProviderGUID);
      Type ^type = Type::GetTypeFromCLSID(Guid(IDgnSiteGUID));

      Object ^idgnSite = Activator::CreateInstance(type);      
      IntPtr i = Marshal::GetIUnknownForObject(idgnSite);

      try {
         // http://stackoverflow.com/a/22160325/1254575
         Marshal::QueryInterface(i, iServiceProviderGuid, sitePtr);
      } finally {
         Marshal::Release(i);
      }

      return sitePtr;
   }

   void NatSpeakService::Disconnect() {
	   Trace::WriteLine(__FUNCTION__);
   }

   /// <summary>
   /// Gets the the profile name of the current Dragon user.
   /// </summary>
   /// <returns>The dragon profile name, if available. null otherwise.</returns>
   String ^NatSpeakService::GetCurrentUserProfileName() {
      ISrSpeaker ^isrSpeaker = (ISrSpeaker ^) _isrCentral;
      
      DWORD dwSize, dwNeeded = 0;
      PWSTR profileName = nullptr;

      // Find out how big our buffer should be
      try {
         isrSpeaker->Query(profileName, 0, &dwNeeded);
      } catch (COMException ^e) {
         if (!(e->ErrorCode == EVENT_E_CANT_MODIFY_OR_DELETE_CONFIGURED_OBJECT ||
               e->ErrorCode == E_BUFFERTOOSMALL || e->ErrorCode == SRERR_NOUSERSELECTED)) {
            throw;
         }
      }

      if (dwNeeded == 0)
         return nullptr;

      // Allocate a buffer to hold the string
      dwSize = dwNeeded;
      profileName = new WCHAR[dwSize];

      // Get the string
      isrSpeaker->Query(profileName, dwSize, &dwNeeded);

      try {
         return gcnew String(profileName);
      } finally {
         delete profileName;
      }
   }

   /// <summary>
   /// Gets the the file system path to the specified Dragon user's profile directory.
   /// </summary>
   /// <param name="userProfile">The name of the user profile to look up.</param>
   /// <returns>The dragon profile path, if available. null otherwise.</returns>
   String ^NatSpeakService::GetUserDirectory(String ^userProfile) {
      IDgnSrSpeaker ^idgnSrSpeaker = (IDgnSrSpeaker ^) _isrCentral;

      DWORD dwSize, dwNeeded = 0;
      PWSTR path = nullptr;

      pin_ptr<const WCHAR> user = PtrToStringChars(userProfile);

      // Find out how big our buffer should be
      try {
         idgnSrSpeaker->GetSpeakerDirectory(user, path, 0, &dwNeeded);
      } catch (COMException ^e) {
         if (!(e->ErrorCode == EVENT_E_CANT_MODIFY_OR_DELETE_CONFIGURED_OBJECT ||
               e->ErrorCode == E_BUFFERTOOSMALL || e->ErrorCode == SRERR_NOUSERSELECTED)) {
            throw;
         }
      }

      if (dwNeeded == 0)
         return nullptr;

      // Allocate a buffer to hold the string
      dwSize = dwNeeded;
      path = new WCHAR[dwSize];

      // Get the string
      idgnSrSpeaker->GetSpeakerDirectory(user, path, dwSize, &dwNeeded);

      try {
         return gcnew String(path) + "\\current";
      } finally {
         delete path;
      }
   }

   void NatSpeakService::RegisterEngineSink() {
      // Create an engine sink
      ISrNotifySink ^isrNotifySink = gcnew SrNotifySink();

      // https://msdn.microsoft.com/en-us/library/1dz8byfh.aspx
      pin_ptr<DWORD> _key = &key;

      IntPtr i = Marshal::GetIUnknownForObject(isrNotifySink);

      // Register our notification sink
      _isrCentral->Register(i, __uuidof(ISrNotifySink^), _key);

      Marshal::Release(i);
   }

   void NatSpeakService::RegisterSpeechServiceSinks() {
      IDgnSSvcActionNotifySink ^playbackSink = gcnew SSvcActionNotifySink();

      // Speech Services
      IDgnSpeechServices ^*ptr = ComHelper::QueryService<ISpchServices^, IDgnSpeechServices^>(_piServiceProvider);
      _idgnSpeechServices = (IDgnSpeechServices^) Marshal::GetObjectForIUnknown(IntPtr(ptr));
      _idgnSSvcOutputEvent = (IDgnSSvcOutputEvent ^) _idgnSpeechServices;
      _idgnSSvcInterpreter = (IDgnSSvcInterpreter ^) _idgnSSvcOutputEvent;

      IntPtr i = Marshal::GetIUnknownForObject(playbackSink);

      _idgnSSvcOutputEvent->Register(i);
      _idgnSSvcInterpreter->Register(i);
      
      Marshal::Release(i);
   }

   void NatSpeakService::ReleaseSiteObject(IntPtr sitePtr) {
      Marshal::Release(sitePtr);
   }
}