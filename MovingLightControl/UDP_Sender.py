#!/usr/bin/python
import socket
import sys, getopt

if len(sys.argv) != 3:  # the program name and the two arguments
  # stop the program and print an error message
  sys.exit("Must provide ip-address and port")

UDP_IP = sys.argv[1]
UDP_PORT = int(sys.argv[2])

print "UDP target IP:", UDP_IP
print "UDP target port:", UDP_PORT

while True:
	
	MESSAGE = raw_input("Enter Message to send: ")
	if MESSAGE=="quit":
		print "Closing..."
		sys.exit(0)
	print "You entered :", MESSAGE
	sock = socket.socket( socket.AF_INET, # Internet
	                  socket.SOCK_DGRAM ) # UDP
	sock.sendto( MESSAGE, (UDP_IP, UDP_PORT) )
	print "Message was successfully sent!"
