/interface bridge
add name=bridge
/ip pool
add name=dhcp_pool0 ranges=192.168.10.32-192.168.10.254
/port
set 0 name=serial0
/ppp profile
add bridge=bridge local-address=192.168.10.1 name=openvpn remote-address=\
    dhcp_pool0
/interface bridge port
{{ for i in 3..8 }}
add bridge=bridge interface=ether{{ i }}
{{ end }}
/ip address
add address=192.168.10.1/24 interface=bridge network=192.168.10.0
add address=192.168.11.1/24 interface=bridge network=192.168.11.0
add address=192.168.12.1/24 interface=bridge network=192.168.12.0
/ip dhcp-client
add interface=ether1
add add-default-route=no interface=ether2 use-peer-dns=no
/ip dhcp-server
add address-pool=dhcp_pool0 interface=bridge name=dhcp1
/ip dhcp-server network
add address=192.168.10.0/24 dns-server=192.168.10.1 gateway=192.168.10.1
/ip dns
set allow-remote-requests=yes
/ip firewall nat
add action=masquerade chain=srcnat out-interface=ether1
/ip service
set www-ssl certificate=router disabled=no
/ppp secret
add name=openvpn password=openvpn profile=openvpn
/system identity
set name={{ host.params.identity }}
