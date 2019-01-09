#ifndef WRAPPER_EXITCODE_H
#define WRAPPER_EXITCODE_H

namespace Wrapper
{
#define ENGINE_EXITCODE( name, value ) name = value,

enum class ExitCode : int
{
#include "ExitCodes.inc"
};

#undef ENGINE_EXITCODE

inline const char* ExitCodeToString( ExitCode exitCode )
{
#define ENGINE_EXITCODE( name, value ) case ExitCode::name: return #name;

	switch( exitCode )
	{
#include "ExitCodes.inc"
	default: return "Unknown Error Code";
	}

#undef ENGINE_EXITCODE
}
}

#endif //WRAPPER_EXITCODE_H
