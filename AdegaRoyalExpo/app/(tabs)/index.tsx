import React, { useEffect, useState } from 'react';
import { StyleSheet, Text, View, TouchableOpacity, ActivityIndicator } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import { useRouter } from 'expo-router';
import { authService, DecodedToken } from '@/services/authService';

export default function HomeScreen() {
  const router = useRouter();
  const [user, setUser] = useState<DecodedToken | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadUser();
  }, []);

  const loadUser = async () => {
    const decoded = await authService.getUser();
    setUser(decoded);
    setLoading(false);

    if (!decoded) {
      router.replace('/(auth)/login');
    }
  };

  const handleLogout = async () => {
    await authService.logout();
    router.replace('/(auth)/login');
  };

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <LinearGradient colors={['#2A0800', '#120000', '#000000']} style={styles.background} />
        <ActivityIndicator size="large" color="#D4AF37" />
      </View>
    );
  }

  if (!user) return null;

  return (
    <View style={styles.container}>
      <LinearGradient colors={['#2A0800', '#120000', '#000000']} style={styles.background} />

      <View style={styles.content}>
        <View style={styles.avatarContainer}>
          <LinearGradient
            colors={['#D4AF37', '#B8860B']}
            style={styles.avatarGradient}
          >
            <Text style={styles.avatarText}>
              {user.name?.charAt(0).toUpperCase() || '?'}
            </Text>
          </LinearGradient>
        </View>

        <Text style={styles.greeting}>Bem-vindo de volta,</Text>
        <Text style={styles.userName}>{user.name}</Text>

        <View style={styles.infoCard}>
          <View style={styles.infoRow}>
            <Ionicons name="mail-outline" size={18} color="#D4AF37" />
            <Text style={styles.infoText}>{user.email}</Text>
          </View>
          <View style={styles.divider} />
          <View style={styles.infoRow}>
            <Ionicons name="shield-checkmark-outline" size={18} color="#D4AF37" />
            <Text style={styles.infoText}>{user.role}</Text>
          </View>
        </View>

        <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
          <Ionicons name="log-out-outline" size={20} color="#D4AF37" />
          <Text style={styles.logoutText}>Sair da conta</Text>
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  background: {
    position: 'absolute',
    left: 0,
    right: 0,
    top: 0,
    bottom: 0,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  content: {
    flex: 1,
    paddingHorizontal: 30,
    justifyContent: 'center',
    alignItems: 'center',
  },
  avatarContainer: {
    marginBottom: 30,
  },
  avatarGradient: {
    width: 100,
    height: 100,
    borderRadius: 50,
    justifyContent: 'center',
    alignItems: 'center',
    shadowColor: '#D4AF37',
    shadowOffset: { width: 0, height: 6 },
    shadowOpacity: 0.4,
    shadowRadius: 12,
    elevation: 10,
  },
  avatarText: {
    fontSize: 42,
    fontWeight: 'bold',
    color: '#120000',
  },
  greeting: {
    fontSize: 16,
    color: '#A0A0A0',
    letterSpacing: 1,
  },
  userName: {
    fontSize: 32,
    fontWeight: '300',
    color: '#D4AF37',
    marginTop: 5,
    letterSpacing: 1,
    textAlign: 'center',
  },
  infoCard: {
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    borderRadius: 16,
    padding: 20,
    width: '100%',
    marginTop: 40,
    borderWidth: 1,
    borderColor: 'rgba(212, 175, 55, 0.15)',
  },
  infoRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    paddingVertical: 8,
  },
  infoText: {
    color: '#CCC',
    fontSize: 15,
  },
  divider: {
    height: 1,
    backgroundColor: 'rgba(212, 175, 55, 0.1)',
    marginVertical: 6,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    marginTop: 40,
    paddingVertical: 14,
    paddingHorizontal: 30,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: 'rgba(212, 175, 55, 0.3)',
  },
  logoutText: {
    color: '#D4AF37',
    fontSize: 16,
    fontWeight: '500',
  },
});
