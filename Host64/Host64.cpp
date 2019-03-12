// Host64.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include "pch.h"
#include "renfrew64_h.h"

// ReSharper disable once CppInconsistentNaming
/**
 * \brief Throws an error code if not equal to 0.
 * \param a The error code to throw.
 */
#define throw_on_err(a) if ((a)) throw (a) // NOLINT(hicpp-exception-baseclass)

// RPC endpoint name. To be used across RPC server/clients.
const TCHAR pctszEndpointName[] = _T("DD8A3E19-F2F2-4B24-81F1-816B1D6126D8");

/**
 *
 */
LPTSTR get_error_message(DWORD status) {
   LPTSTR message = nullptr;

   DWORD r = FormatMessage(
      FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
      nullptr,
      status,
      0, // Language ID is deprecated, just use zero.
      reinterpret_cast<LPTSTR>(&message),
      0,
      nullptr
   );

   if (r == 0 || message == nullptr)
      return _tcsdup(_T("An unknown error occurred."));

   return _tcsdup(message);
}

void print_error(RPC_STATUS status) {
   if (!status)
      return;

   const LPTSTR message = get_error_message(status);

   std::tcout << "I leave you with the following words: " << message << std::endl;

   free(message);
}

/**
 *
 */
void launch_rpc_listener(RPC_IF_HANDLE ifSpec) {
   LPTSTR ptszProtSeq = _tcsdup(_T("ncalrpc"));
   LPTSTR ptszEndpoint = _tcsdup(pctszEndpointName);

   try {
      throw_on_err(RpcServerUseProtseqEp(
         reinterpret_cast<RPC_TSTR>(ptszProtSeq),
         RPC_C_LISTEN_MAX_CALLS_DEFAULT,
         reinterpret_cast<RPC_TSTR>(ptszEndpoint), nullptr
      ));

      throw_on_err(
         RpcServerRegisterIf(ifSpec, nullptr, nullptr)
      );

      throw_on_err(
         RpcServerListen(1, RPC_C_LISTEN_MAX_CALLS_DEFAULT, FALSE)
      );
   } catch (RPC_STATUS e) {
      print_error(e);
   }

   free(ptszProtSeq);
   free(ptszEndpoint);
}

/**
 *
 */
int main() {

   // Start our listener(s).
   launch_rpc_listener(Ping_v1_0_s_ifspec);
   launch_rpc_listener(Magnifier_v1_0_s_ifspec);

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
