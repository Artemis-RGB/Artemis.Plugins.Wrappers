#pragma once
#include <vector>
#include <cassert>

template<uint32_t Command>
class ArtemisVariableBuffer
{
private:
	static constexpr int HeaderSize = sizeof(uint32_t) + sizeof(uint32_t);
	std::vector<uint8_t> buffer;
	size_t offset;

public:
	ArtemisVariableBuffer(size_t dataSize) : buffer(dataSize + HeaderSize, 0), offset(0)
	{
		write((uint32_t)size());
		write((uint32_t)Command);
	}

	const uint8_t* data() const
	{
		return buffer.data();
	}

	const size_t size() const
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

	template<typename T>
	void write(const T* data, const size_t count)
	{
		const auto dataSize = sizeof(T) * count;

		assert(offset + dataSize <= size());

		std::memcpy(&buffer[offset], data, dataSize);
		offset += dataSize;
	}
};