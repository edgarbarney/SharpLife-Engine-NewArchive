/**
*	@file This header is used to define all exit codes used by the engine
*	The macro ENGINE_EXITCODE must be defined by users to receive each exit code name and value
*	Must match the ExitCode enum in the managed engine
*/

ENGINE_EXITCODE( Success, 0 )
ENGINE_EXITCODE( UnknownError, 1 )
ENGINE_EXITCODE( WrapperError, 2 )
ENGINE_EXITCODE( NoCommandLineArguments, 3 )
ENGINE_EXITCODE( UnhandledException, 4 )
