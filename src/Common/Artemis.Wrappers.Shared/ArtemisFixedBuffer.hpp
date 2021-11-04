#pragma once
#include <array>
#include <cassert>

template<uint32_t Command, size_t DataSize>
class ArtemisFixedBuffer
{
private:
	static constexpr int HeaderSize = sizeof(uint32_t) + sizeof(uint32_t);
	std::array<uint8_t, HeaderSize + DataSize> buffer;
	size_t offset;

public:
	constexpr ArtemisFixedBuffer() : buffer({ 0 }), offset(0)
	{
		write((uint32_t)size());
		write((uint32_t)Command);
	}

	const uint8_t* data() const
	{
		return buffer.data();
	}

	constexpr size_t size() const
	{
		return buffer.size();
	}

	constexpr void write(const uint32_t& data)
	{
		assert(offset <= size());

		buffer[offset++] = (data & 0x000000ff);
		buffer[offset++] = (data & 0x0000ff00) >> 8;
		buffer[offset++] = (data & 0x00ff0000) >> 16;
		buffer[offset++] = (data & 0xff000000) >> 24;
	}

	constexpr void write(const int32_t& data)
	{
		assert(offset <= size());

		buffer[offset++] = (data & 0x000000ff);
		buffer[offset++] = (data & 0x0000ff00) >> 8;
		buffer[offset++] = (data & 0x00ff0000) >> 16;
		buffer[offset++] = (data & 0xff000000) >> 24;
	}

	constexpr void write(const uint8_t& data) {
		assert(offset <= size());

		buffer[offset++] = data;
	}

	template<typename T> requires !std::is_pointer_v<T>
	inline void write(const T& data)
	{
		constexpr auto size = sizeof(T);

		assert(offset + size <= buffer.size());

		std::memcpy(&buffer[offset], &data, size);
		offset += size;
	}
};