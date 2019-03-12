

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.01.0622 */
/* at Mon Jan 18 22:14:07 2038
 */
/* Compiler settings for ..\RPC\renfrew64.idl, D:\Opt\Projects\Project Renfrew\RPC\renfrew64.acf:
    Oicf, W1, Zp8, env=Win64 (32b run), target_arch=AMD64 8.01.0622 
    protocol : all , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */



/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 500
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif /* __RPCNDR_H_VERSION__ */


#ifndef __renfrew64_h_h__
#define __renfrew64_h_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __Magnifier_INTERFACE_DEFINED__
#define __Magnifier_INTERFACE_DEFINED__

/* interface Magnifier */
/* [implicit_handle][type_strict_context_handle][version][uuid] */ 

void mag_Echo( 
    /* [string][in] */ const TCHAR *ptszMessage);


extern handle_t magnifier_IfHandle;


extern RPC_IF_HANDLE Magnifier_v1_0_c_ifspec;
extern RPC_IF_HANDLE Magnifier_v1_0_s_ifspec;
#endif /* __Magnifier_INTERFACE_DEFINED__ */

#ifndef __Ping_INTERFACE_DEFINED__
#define __Ping_INTERFACE_DEFINED__

/* interface Ping */
/* [implicit_handle][type_strict_context_handle][version][uuid] */ 

int ping_Ping( void);


extern handle_t ping_IfHandle;


extern RPC_IF_HANDLE Ping_v1_0_c_ifspec;
extern RPC_IF_HANDLE Ping_v1_0_s_ifspec;
#endif /* __Ping_INTERFACE_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


