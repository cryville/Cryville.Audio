%module oboe

%include "stdint.h"
%include "sys/types.h"

%include <std_shared_ptr.i>
%include <std_string.i>

%template(AudioStreamPtr) std::shared_ptr<oboe::AudioStream>;

%inline %{
#include "Definitions.h"
#include "AudioStreamCallback.h"
#include "ResultWithValue.h"
#include "LatencyTuner.h"
#include "AudioStreamBase.h"
#include "AudioStream.h"
#include "AudioStreamBuilder.h"
#include "Utilities.h"
#include "Version.h"
#include "StabilizedCallback.h"
#include "Oboe.h"
%}

%include "Definitions.h"
%include "AudioStreamCallback.h"
%include "ResultWithValue.h"
%include "LatencyTuner.h"
%include "AudioStreamBase.h"
%include "AudioStream.h"
%include "AudioStreamBuilder.h"
%include "Utilities.h"
%include "Version.h"
%include "StabilizedCallback.h"
%include "Oboe.h"

%template(ResultWithDouble) oboe::ResultWithValue<double>;
%template(ResultWithInt) oboe::ResultWithValue<int32_t>;
