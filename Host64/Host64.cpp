// Host64.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include "renfrew64_h.h"

const TCHAR pctszEndpointName[] = _T("DD8A3E19-F2F2-4B24-81F1-816B1D6126D8");

/**
 *
 */
LPTSTR get_error_message(int errorCode) {
   LPTSTR message = nullptr;

   DWORD r = FormatMessage(
      FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
      nullptr,
      errorCode,
      MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
      reinterpret_cast<LPTSTR>(&message),
      0,
      nullptr
   );

   if (r == 0 || message == nullptr)
      return _tcsdup(_T("An unknown error occurred."));

   return _tcsdup(message);
}

void die_on_err(RPC_STATUS status) {
   if (!status)
      return;

   const LPTSTR message = get_error_message(status);

   std::tcout << "I leave you with the following words: " << message << std::endl;

   free(message);

   exit(status);
}

/**
 *
 */
void launch_mag_listener() {
   RPC_STATUS r;

   LPTSTR ptszProtSeq = _tcsdup(_T("ncalrpc"));
   LPTSTR ptszEndpoint = _tcsdup(pctszEndpointName);

   r = RpcServerUseProtseqEp(
      reinterpret_cast<RPC_TSTR>(ptszProtSeq),
      RPC_C_LISTEN_MAX_CALLS_DEFAULT,
      reinterpret_cast<RPC_TSTR>(ptszEndpoint), nullptr
   );

   die_on_err(r);

   r = RpcServerRegisterIf(Magnifier_v1_0_s_ifspec, nullptr, nullptr);

   die_on_err(r);

   r = RpcServerListen(1, RPC_C_LISTEN_MAX_CALLS_DEFAULT, FALSE);

   die_on_err(r);

   // TODO: Free on death.
   free(ptszProtSeq);
   free(ptszEndpoint);
}

/**
 *
 */
int main() {

   // Start our listener(s).
   launch_mag_listener();

   // Stay resident, but don't peg the CPU.
   while (true)
      std::this_thread::sleep_for(std::chrono::milliseconds(100));
}

/**
 *
 */
void __RPC_FAR * __RPC_USER midl_user_allocate(size_t len) {
   return(malloc(len));
}

/**
 *
 */
void __RPC_USER midl_user_free(void __RPC_FAR * ptr) {
   free(ptr);
}
