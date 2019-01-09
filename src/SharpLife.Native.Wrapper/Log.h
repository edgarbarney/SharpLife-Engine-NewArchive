#ifndef WRAPPER_LOG_H
#define WRAPPER_LOG_H

#include <string>

namespace Wrapper
{
namespace Log
{
void Message( const char* pszFormat, ... );

void DebugMessage( const char* pszFormat, ... );

void SetDebugLoggingEnabled( bool bEnable );

void SetGameDirectory( const std::string_view& szGameDir );
}
}

#endif //WRAPPER_LOG_H
