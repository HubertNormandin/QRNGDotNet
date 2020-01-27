# QRNGDotNet

QRNGDotNet was developed to provide access to .NET developper with fast and concurrent implementation of quasi-random number generator also known as low-discrepancy sequences. QRNGDotNet strive for performance and was design to leverage multiprocessor architecture. The implementations provided in this library inherited the System.Random class making them compatible with <span>Math.</span>Net Numerics distributions. Currently, QRNGDotNet contain only an implementation of the Sobol Sequences. This implementation rely on Stephen Joe and Frances Y. Kuo initial direction numbers available at https://web.maths.unsw.edu.au/~fkuo/sobol/index.html. An effort is currently made to improve the concurrency and reduce some of the initialization overhead.
 
 ***
Here is a list of what will be added in the future:
- [ ] Documentation: The current implementation is partially documented.
- [ ] Samples and userguide: Samples showing the uses of the library.
- [ ] Implementation of other low-discrepancy sequences to be determined. @HubertNormandin is currently looking into the feasability of implementing extensible (t,s)-Sequences with a leap-frog algorithm.


***
### User Guide
#### Adding Initialization Numbers
While the initialization numbers included in the implementation are currently optimal, it is possible to add new initialization numbers by modifying the Config.cs file and adding the new initialization numbers to the Resources.resx file in the properties directory. Some users might want to generate their own initialization numbers. In this case the best solution would be to override load the DirectionNumber class.

