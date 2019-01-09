#include "CManagedHost.h"
#include "CLR/CCLRHostException.h"
#include "ConfigurationInput.h"
#include "ExitCode.h"
#include "Log.h"
#include "Utility/StringUtils.h"

namespace Wrapper
{
const std::string_view CManagedHost::CONFIG_FILENAME{ "cfg/SharpLife-Wrapper-Native.ini" };

using ManagedEntryPoint = ExitCode( STDMETHODCALLTYPE* )( bool bIsServer );

CManagedHost::CManagedHost() = default;

CManagedHost::~CManagedHost() = default;

void CManagedHost::Initialize( std::string&& szGameDir, bool bIsServer )
{
	m_szGameDir = std::move( szGameDir );
	m_bIsServer = bIsServer;

	Log::SetGameDirectory( m_szGameDir );

	Log::DebugMessage( "Managed host initialized with game directory %s (%s)", m_szGameDir.c_str(), bIsServer ? "server" : "client" );
}

void CManagedHost::Start()
{
	auto exitCode = ExitCode::WrapperError;

	if( LoadConfiguration() )
	{
		Log::DebugMessage( "Configuration loaded" );

		if( StartManagedHost() )
		{
			Log::DebugMessage( "Managed host started" );

			try
			{
				Log::DebugMessage( "Attempting to load assembly and acquire entry point" );

				auto entryPoint = reinterpret_cast< ManagedEntryPoint >( m_CLRHost->LoadAssemblyAndGetEntryPoint(
					Utility::ToWideString( m_Configuration.ManagedEntryPoint.AssemblyName ),
					Utility::ToWideString( m_Configuration.ManagedEntryPoint.Class ),
					Utility::ToWideString( m_Configuration.ManagedEntryPoint.Method )
				) );

				Log::DebugMessage( "Attempting to execute entry point" );

				exitCode = entryPoint( m_bIsServer );

				Log::DebugMessage( "Entry point executed with exit code %d", exitCode );
			}
			catch( const CLR::CCLRHostException& e )
			{
				if( e.HasResultCode() )
				{
					Log::Message( "ERROR - %s\nError code:%x", e.what(), e.GetResultCode() );
				}
				else
				{
					Log::Message( "ERROR - %s", e.what() );
				}
			}

			Log::DebugMessage( "Shutting down managed host" );

			ShutdownManagedHost();
		}
	}

	Log::DebugMessage( "Exiting with code %s (%d)", ExitCodeToString( exitCode ), exitCode );

	std::quick_exit( ( int ) exitCode );
}

bool CManagedHost::LoadConfiguration()
{
	auto config = Wrapper::LoadConfiguration( m_szGameDir + '/' + std::string{ CONFIG_FILENAME } );

	if( config )
	{
		m_Configuration = std::move( config.value() );
		Log::SetDebugLoggingEnabled( m_Configuration.DebugLoggingEnabled );
		return true;
	}

	return false;
}

bool CManagedHost::StartManagedHost()
{
	auto dllsPath = Utility::ToWideString( m_szGameDir ) + L'/' + Utility::ToWideString( m_Configuration.ManagedEntryPoint.Path );

	dllsPath = Utility::GetAbsolutePath( dllsPath );

	try
	{
		m_CLRHost = std::make_unique<CLR::CCLRHost>( dllsPath, m_Configuration.SupportedDotNetCoreVersions );
	}
	catch( const CLR::CCLRHostException e )
	{
		if( e.HasResultCode() )
		{
			Log::Message( "ERROR - %s\nError code:%x", e.what(), e.GetResultCode() );
		}
		else
		{
			Log::Message( "ERROR - %s", e.what() );
		}

		return false;
	}

	return true;
}

void CManagedHost::ShutdownManagedHost()
{
	m_CLRHost.release();
}
}
