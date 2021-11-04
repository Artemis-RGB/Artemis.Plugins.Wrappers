#pragma once
#include "ArtemisFixedBuffer.hpp"
#include "ArtemisVariableBuffer.hpp"

/// <summary>
/// Helper class to create ArtemisBuffers.
/// Prevents typos since the size of each buffer
/// is calculated from the arguments themselves
/// instead of being manually added first.
/// </summary>
template<typename TEnum> requires std::is_enum_v<TEnum>&& std::is_same_v<std::underlying_type_t<TEnum>, uint32_t>
class ArtemisBuffer {
private:
	ArtemisBuffer() {}
public:
	/// <summary>
	/// Creates a buffer of adequate size depending on the types 
	/// of the parameters provided. The size of each buffer is 
	/// a compile time constant, so we can use a fixed sized 
	/// array for these.
	/// </summary>
	template<TEnum Command, typename... Args>
	static auto create(Args ... args)
	{
		//this gets the sum of sizeof() for all arguments,
		//giving us the size needed to store them all
		constexpr int size = (sizeof(args) + ... + 0);

		//then we use the size we computed above to create a buffer
		ArtemisFixedBuffer<Command, size> buffer;

		//we then use this fold expression to write each argument
		(buffer.write(args), ...);

		//when we get here, the buffer was constructed and filled up
		return buffer;
	}

	/// <summary>
	/// Special case for a buffer that carries no 
	/// additional data besides the command
	/// </summary>
	template<TEnum Command>
	static auto constexpr create()
	{
		return ArtemisFixedBuffer<Command, 0>();
	}

	/// <summary>
	/// Special case for a buffer that carries 
	/// an inner array of known size
	/// </summary>
	template<TEnum Command, size_t Size>
	static auto create(const uint8_t* data)
	{
		ArtemisFixedBuffer<Command, Size> buffer;

		for (size_t i = 0; i < Size; i++)
		{
			buffer.write(data[i]);
		}

		return buffer;
	}

	/// <summary>
	/// Creates a buffer with size not known at compile
	/// time. It will use std::vector instead of std::array,
	/// but is otherwise equivalent. It's used for Init and 
	/// Shutdown as those have variable-length strings.
	/// </summary>
	template<TEnum Command, typename T>
	static auto create_dynamic(const T* data, const size_t count)
	{
		ArtemisVariableBuffer<Command> buffer(count * sizeof(T));

		buffer.write(data, count);

		return buffer;
	}
};