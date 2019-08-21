import skrf as rf
import matplotlib.pyplot as plt
from scipy import constants
import numpy as np
import sys
 
raw_points = 101
NFFT = 16384
PROPAGATION_SPEED = 65.9 #For RG405
plt_data = sys.argv[1]
print(plt_data)
 #Read skrf docs
_prop_speed = PROPAGATION_SPEED/100
cable = rf.Network(plt_data)

print(cable)
s11 = cable.s[:, 0, 0]
print(type(s11))
window = np.blackman(raw_points)
s11 = window * s11
td = np.abs(np.fft.ifft(s11, NFFT))
#print(s11)
 
#Calculate maximum time axis
t_axis = np.linspace(0, 1/cable.frequency.step, NFFT)
d_axis = constants.speed_of_light * _prop_speed * t_axis
 
#find the peak and distance
pk = np.max(td)
idx_pk = np.where(td == pk)[0]
cable_len  = d_axis[idx_pk[0]]/2
#print(cable_len)
#print(d_axis) 
# Plot time response
plt.plot(d_axis, td)
plt.xlabel("Distance (m) Length of cable(%.2fm)" % cable_len)
plt.ylabel("Magnitude")
plt.title("Return loss Time domain")
plt.show()