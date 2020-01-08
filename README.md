QRNGDotNet was developed to provide access to .NET developper with fast and concurrent implementation of quasi-random number generator also known as low-discrepency sequences. QRNGDotNet strive for performance and was design to leverage multiprocessor architecture. The implementations provided in this library inherited the System.Random class making them compatible with <span>Math.</span>Net Numerics distributions. Currently, QRNGDotNet contain an implementation of the Sobol Sequences. This implementation rely on Stephen Joe and Frances Y. Kuo initial direction numbers available at https://web.maths.unsw.edu.au/~fkuo/sobol/index.html. Other initialization number can be added by modifying the Config.cs file and adding them to the resource. 
 
Here is a list of what will be added in the future:
- [ ] Documentation: The current implementation is partially documented.
- [ ] Samples and userguide: Samples showing the uses of the library.
- [ ] Implementation of other low-discrepency sequences to be determined. @HubertNormandin is currently looking into the feasability of implementing extensible (t,s)-Sequences with a leap-frog algorithm.

An effort is currently made to improve the concurrency and reduce some of the initialization overhead
