#include <cassert>
#include <chrono>
#include <cstdarg>
#include <cstdio>
#include <ctime>
#include <filesystem>
#include <fstream>
#include <iomanip>
#include <memory>

#include "Log.h"

namespace Wrapper
{
namespace Log
{
const std::string LOG_FILENAME{ "logs/SharpLifeWrapper-Native.log" };

std::string LOG_FILE_PATH;

static void LogToFile( const char* pszFormat, va_list list )
{
	assert( !LOG_FILE_PATH.empty() );

	if( std::ofstream file{ LOG_FILE_PATH, std::ofstream::app }; file )
	{
		auto time = std::time( nullptr );

		auto local = *std::localtime( &time );

		file << std::put_time( &local, "[%d/%m/%Y %T %z]: " );

		//Avoid trashing the list object
		va_list testCopy;

		va_copy( testCopy, list );

		auto needed = vsnprintf( nullptr, 0, pszFormat, testCopy );

		if( 0 <= needed )
		{
			auto buffer = std::make_unique<char[]>( needed + 1 );

			vsnprintf( buffer.get(), needed + 1, pszFormat, list );

			file << buffer.get();
		}
		else
		{
			file << "Error formatting output with code " << needed << " and format string " << pszFormat;
		}

		file << std::endl;

		file.flush();
	}
	else
	{
		assert( !"Couldn't open log file for writing" );
	}
}

//Starts off enabled in case anything happens before the configuration is loaded
static bool g_bDebugLoggingEnabled = true;

void Message( const char* pszFormat, ... )
{
	va_list list;

	va_start( list, pszFormat );

	LogToFile( pszFormat, list );

	va_end( list );
}

void DebugMessage( const char* pszFormat, ... )
{
	if( g_bDebugLoggingEnabled )
	{
		va_list list;

		va_start( list, pszFormat );

		LogToFile( pszFormat, list );

		va_end( list );
	}
}

void SetDebugLoggingEnabled( bool bEnable )
{
	g_bDebugLoggingEnabled = bEnable;
}

void SetGameDirectory( const std::string_view& szGameDir )
{
	//Convert to string to avoid any additional memory usage in specific path implementations
	LOG_FILE_PATH = ( std::filesystem::path( szGameDir ) / LOG_FILENAME ).string();
	LOG_FILE_PATH.shrink_to_fit();
}
}
}
