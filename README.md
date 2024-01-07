# 1️⃣🐝🏎️ The One Billion Row Challenge

> The One Billion Row Challenge (1BRC [Original](https://github.com/gunnarmorling/1brc)) is a fun exploration of how far modern .NET can be pushed for aggregating one billion rows from a text file.
> Grab all your (virtual) threads, reach out to SIMD, optimize your GC, or pull any other trick, and create the fastest implementation for solving this task!

## Solutions

Tested on 3.60GHz AMD Ryzen 5 3600 6-Core Processor | 16.0 GB (3GB usable) | Win-x64 | Windows 11 Pro 22H2

Ranking are based on the results generated on my local machine - they may differ (improve) in other environment

| #. | Results (MM:SS:MS) | Implementation                                                                                                   | Runtime | Submitter                                                                                               |
|----|--------------------|------------------------------------------------------------------------------------------------------------------|---------|---------------------------------------------------------------------------------------------------------|
| 1. | 04:29.89           | [Naive](https://github.com/KoditkarVedant/1brc/tree/main/src/OneBRC/Naive)                                       | Win-x64 | [Me](https://github.com/KoditkarVedant)                                                                 |
| 2. | 09:00.59           | [ParallelMemoryMappedFile](https://github.com/KoditkarVedant/1brc/tree/main/src/OneBRC/ParallelMemoryMappedFile) | Win-x64 | [Me](https://github.com/KoditkarVedant) [Inspiration - Victor Baybekov] (https://github.com/buybackoff) |
| X. | Need 20+ GB RAM    | [Dumb](https://github.com/KoditkarVedant/1brc/tree/main/src/OneBRC/Dumb)                                         | Win-x64 | [Me](https://github.com/KoditkarVedant)                                                                 |
